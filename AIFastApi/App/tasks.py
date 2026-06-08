import os

from celery import Celery

from App.model_service import get_predictor


REDIS_URL = os.getenv("REDIS_URL", "redis://taskflow-redis:6379/0")

celery_app = Celery(
    "tasks",
    broker=REDIS_URL,
    backend=REDIS_URL,
)

@celery_app.task
def predict_task_priority(title: str, description: str):
    predictor = get_predictor()
    return predictor.predict(title, description)
