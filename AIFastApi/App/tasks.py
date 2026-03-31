from celery import Celery
from App.model_service import get_predictor

celery_app = Celery(
    "tasks",
    broker="redis://taskflow-redis:6379/0", 
    backend="redis://taskflow-redis:6379/0"
)

@celery_app.task
def predict_task_priority(title: str, description: str):
    predictor = get_predictor()
    return predictor.predict(title, description)
