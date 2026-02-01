from fastapi import FastAPI, UploadFile, File
from fastapi.responses import FileResponse
from fastapi.staticfiles import StaticFiles
import os, shutil, uuid

UPLOAD_DIR = "/data/uploads"
os.makedirs(UPLOAD_DIR, exist_ok=True)

app = FastAPI()

@app.post("/api/upload")
async def upload(file: UploadFile = File(...)):
    ext = os.path.splitext(file.filename)[1]
    name = f"{uuid.uuid4().hex}{ext}"
    path = os.path.join(UPLOAD_DIR, name)

    with open(path, "wb") as f:
        shutil.copyfileobj(file.file, f)

    return {"filename": name, "directUrl": f"/files/{name}"}

@app.get("/api/files/download/{filename}")
def download(filename: str):
    path = os.path.join(UPLOAD_DIR, filename)
    if not os.path.isfile(path):
        return {"error": "not found"}
    return FileResponse(path)

app.mount("/files", StaticFiles(directory=UPLOAD_DIR), name="files")
