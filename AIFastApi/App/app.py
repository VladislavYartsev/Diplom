from fastapi import FastAPI
from pydantic import BaseModel
from App.tasks import predict_task_priority, celery_app
from celery import Celery
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI()

origins = [
    "http://localhost:5000",  
    "http://127.0.0.1:5000",
    "*" 
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],   
    allow_headers=["*"],
)

class TaskRequest(BaseModel):
    title: str
    description: str

@app.get("/health")
def health_check():
    try:
        res = celery_app.control.ping(timeout=1)
        if res:
            return {"status": "healthy", "celery": "up"}
        else:
            return {"status": "unhealthy", "celery": "down"}
    except Exception:
        return {"status": "unhealthy", "celery": "down"}

@app.post("/predict")
def predict_priority(task: TaskRequest):
    celery_task = predict_task_priority.delay(task.title, task.description)
    
    return {"task_id": celery_task.id}

@app.get("/predict/{task_id}")
def get_prediction(task_id: str):

    result = celery_app.AsyncResult(task_id)
    if result.ready():
        return {"priority": result.result}
    else:
        return {"status": result.status}