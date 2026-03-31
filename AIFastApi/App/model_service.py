import json
import os
import pickle
from pathlib import Path


BASE_DIR = Path(__file__).resolve().parent.parent
DEFAULT_SKLEARN_MODEL_PATH = BASE_DIR / "kanban_priority_model.pkl"
DEFAULT_RUBERT_DIR = BASE_DIR / "models" / "rubert-priority"


class PriorityPredictor:
    def __init__(self) -> None:
        self.backend = None
        self.model = None
        self.tokenizer = None
        self.labels = None
        self.device = None

        rubert_dir = Path(os.getenv("RUBERT_MODEL_DIR", DEFAULT_RUBERT_DIR))
        if self._try_load_rubert(rubert_dir):
            return

        if self._try_load_sklearn(DEFAULT_SKLEARN_MODEL_PATH):
            return

        raise RuntimeError(
            "No priority model found. Train RuBERT into 'models/rubert-priority' "
            "or place 'kanban_priority_model.pkl' in the project root."
        )

    def _try_load_rubert(self, model_dir: Path) -> bool:
        config_path = model_dir / "label_mapping.json"
        if not model_dir.exists() or not config_path.exists():
            return False

        try:
            import torch
            from transformers import AutoModelForSequenceClassification, AutoTokenizer
        except ImportError:
            return False

        with config_path.open("r", encoding="utf-8") as f:
            payload = json.load(f)

        self.labels = payload["labels"]
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        self.tokenizer = AutoTokenizer.from_pretrained(model_dir)
        self.model = AutoModelForSequenceClassification.from_pretrained(model_dir)
        self.model.to(self.device)
        self.model.eval()
        self.backend = "rubert"
        return True

    def _try_load_sklearn(self, model_path: Path) -> bool:
        if not model_path.exists():
            return False

        with model_path.open("rb") as f:
            self.model = pickle.load(f)

        self.backend = "sklearn"
        return True

    @staticmethod
    def _merge_text(title: str, description: str) -> str:
        return f"{title.strip()} [SEP] {description.strip()}".strip()

    def predict(self, title: str, description: str) -> str:
        text = self._merge_text(title, description)

        if self.backend == "rubert":
            import torch

            inputs = self.tokenizer(
                text,
                truncation=True,
                padding=True,
                max_length=256,
                return_tensors="pt",
            )
            inputs = {key: value.to(self.device) for key, value in inputs.items()}

            with torch.no_grad():
                logits = self.model(**inputs).logits

            predicted_id = int(logits.argmax(dim=-1).item())
            return self.labels[predicted_id]

        prediction = self.model.predict([text])[0]
        return str(prediction)


_predictor: PriorityPredictor | None = None


def get_predictor() -> PriorityPredictor:
    global _predictor
    if _predictor is None:
        _predictor = PriorityPredictor()
    return _predictor
