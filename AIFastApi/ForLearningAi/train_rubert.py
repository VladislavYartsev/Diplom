import argparse
import json
from pathlib import Path

import pandas as pd
import torch
from sklearn.metrics import accuracy_score, classification_report
from sklearn.model_selection import train_test_split
from transformers import (
    AutoModelForSequenceClassification,
    AutoTokenizer,
    Trainer,
    TrainingArguments,
)


BASE_DIR = Path(__file__).resolve().parent.parent
DEFAULT_DATASET_PATH = BASE_DIR / "ForLearningAi" / "TaskFlow_dataset.csv"
DEFAULT_OUTPUT_DIR = BASE_DIR / "models" / "rubert-priority"
DEFAULT_MODEL_NAME = "DeepPavlov/rubert-base-cased"
LABELS = ["Low", "Medium", "High"]
LABEL_TO_ID = {label: idx for idx, label in enumerate(LABELS)}


class TaskPriorityDataset(torch.utils.data.Dataset):
    def __init__(self, texts: list[str], labels: list[int], tokenizer, max_length: int) -> None:
        self.texts = texts
        self.labels = labels
        self.tokenizer = tokenizer
        self.max_length = max_length

    def __len__(self) -> int:
        return len(self.texts)

    def __getitem__(self, idx: int) -> dict[str, torch.Tensor]:
        encoded = self.tokenizer(
            self.texts[idx],
            truncation=True,
            padding="max_length",
            max_length=self.max_length,
            return_tensors="pt",
        )
        item = {key: value.squeeze(0) for key, value in encoded.items()}
        item["labels"] = torch.tensor(self.labels[idx], dtype=torch.long)
        return item


def build_text(title: str, description: str) -> str:
    return f"{title.strip()} [SEP] {description.strip()}".strip()


def load_dataset(csv_path: Path) -> tuple[list[str], list[int]]:
    df = pd.read_csv(csv_path, encoding="utf-8")
    df["Title"] = df["Title"].fillna("")
    df["Description"] = df["Description"].fillna("")
    df["Priority"] = df["Priority"].fillna("").astype(str)
    df = df[df["Priority"].isin(LABEL_TO_ID)]
    df["text"] = df.apply(lambda row: build_text(row["Title"], row["Description"]), axis=1)
    texts = df["text"].tolist()
    labels = [LABEL_TO_ID[label] for label in df["Priority"].tolist()]
    return texts, labels


def compute_metrics(eval_pred) -> dict[str, float]:
    logits, labels = eval_pred
    predictions = logits.argmax(axis=-1)
    return {"accuracy": accuracy_score(labels, predictions)}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", type=Path, default=DEFAULT_DATASET_PATH)
    parser.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR)
    parser.add_argument("--model-name", type=str, default=DEFAULT_MODEL_NAME)
    parser.add_argument("--epochs", type=int, default=3)
    parser.add_argument("--batch-size", type=int, default=8)
    parser.add_argument("--max-length", type=int, default=256)
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    texts, labels = load_dataset(args.dataset)

    X_train, X_test, y_train, y_test = train_test_split(
        texts,
        labels,
        test_size=0.2,
        random_state=42,
        stratify=labels,
    )

    tokenizer = AutoTokenizer.from_pretrained(args.model_name)
    model = AutoModelForSequenceClassification.from_pretrained(
        args.model_name,
        num_labels=len(LABELS),
        id2label={idx: label for label, idx in LABEL_TO_ID.items()},
        label2id=LABEL_TO_ID,
    )

    train_dataset = TaskPriorityDataset(X_train, y_train, tokenizer, args.max_length)
    test_dataset = TaskPriorityDataset(X_test, y_test, tokenizer, args.max_length)

    training_args = TrainingArguments(
        output_dir=str(args.output_dir / "checkpoints"),
        eval_strategy="epoch",
        save_strategy="epoch",
        logging_strategy="epoch",
        num_train_epochs=args.epochs,
        per_device_train_batch_size=args.batch_size,
        per_device_eval_batch_size=args.batch_size,
        learning_rate=2e-5,
        weight_decay=0.01,
        load_best_model_at_end=True,
        metric_for_best_model="accuracy",
        greater_is_better=True,
        report_to="none",
    )

    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=train_dataset,
        eval_dataset=test_dataset,
        compute_metrics=compute_metrics,
    )

    trainer.train()
    predictions = trainer.predict(test_dataset)
    predicted_ids = predictions.predictions.argmax(axis=-1)

    print("Accuracy:", accuracy_score(y_test, predicted_ids))
    print(classification_report(y_test, predicted_ids, target_names=LABELS))

    args.output_dir.mkdir(parents=True, exist_ok=True)
    trainer.save_model(str(args.output_dir))
    tokenizer.save_pretrained(str(args.output_dir))

    mapping = {"labels": LABELS}
    with (args.output_dir / "label_mapping.json").open("w", encoding="utf-8") as f:
        json.dump(mapping, f, ensure_ascii=False, indent=2)

    print(f"Model saved to: {args.output_dir}")


if __name__ == "__main__":
    main()
