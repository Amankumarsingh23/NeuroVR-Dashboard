from fastapi import FastAPI
from pydantic import BaseModel
import math, os
from sqlalchemy import create_engine, Column, String, Float, Integer
from sqlalchemy.orm import declarative_base, sessionmaker
import uuid

app    = FastAPI(title="NeuroVR Dashboard Backend")
DB_URL = os.environ.get("DATABASE_URL", "sqlite:///./neurovr.db")
engine = create_engine(DB_URL)
Base   = declarative_base()
Sess   = sessionmaker(bind=engine)

class SessionRecord(Base):
    __tablename__ = "sessions"
    id            = Column(String,  primary_key=True, default=lambda: str(uuid.uuid4())[:8])
    pupil_avg     = Column(Float,   default=0.0)
    fatigue_avg   = Column(Float,   default=0.0)
    reading_count = Column(Integer, default=0)

Base.metadata.create_all(engine)

class FatigueRequest(BaseModel):
    pupil_diameter: float
    is_blinking:    bool
    timestamp_ms:   int

class FatigueResponse(BaseModel):
    player_id:     str
    fatigue_score: float
    pupil_diameter: float
    timestamp:     str

CURRENT_SESSION_ID = str(uuid.uuid4())[:8]

def compute_fatigue(pupil: float, blinking: bool) -> float:
    pupil_score = 1.0 - min(max(pupil, 0.0), 1.0)
    blink_score = 0.8 if blinking else 0.2
    return round(pupil_score * 0.7 + blink_score * 0.3, 3)

@app.post("/fatigue", response_model=FatigueResponse)
def get_fatigue(req: FatigueRequest):
    score = compute_fatigue(req.pupil_diameter, req.is_blinking)

    db = Sess()
    rec = db.query(SessionRecord).filter_by(id=CURRENT_SESSION_ID).first()
    if not rec:
        rec = SessionRecord(id=CURRENT_SESSION_ID,
                           pupil_avg=req.pupil_diameter,
                           fatigue_avg=score,
                           reading_count=1)
        db.add(rec)
    else:
        n = rec.reading_count
        rec.pupil_avg     = (rec.pupil_avg * n + req.pupil_diameter) / (n + 1)
        rec.fatigue_avg   = (rec.fatigue_avg * n + score) / (n + 1)
        rec.reading_count += 1
    db.commit()
    sid = rec.id
    db.close()

    from datetime import datetime, timezone
    return FatigueResponse(
        player_id=sid,
        fatigue_score=score,
        pupil_diameter=req.pupil_diameter,
        timestamp=datetime.now(timezone.utc).isoformat()
    )

@app.get("/session/{session_id}")
def get_session(session_id: str):
    db = Sess()
    rec = db.query(SessionRecord).filter_by(id=session_id).first()
    db.close()
    if not rec:
        return {"error": "not found"}
    return {"id": rec.id, "pupil_avg": rec.pupil_avg,
            "fatigue_avg": rec.fatigue_avg, "readings": rec.reading_count}

@app.get("/health")
def health(): return {"status": "ok"}
