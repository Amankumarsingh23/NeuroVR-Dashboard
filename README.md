# NeuroVR Dashboard 🧠

> *What if your VR environment could feel when you're tired — before you do?*

A real-time brain fatigue visualisation system built in Unity (C#). It reads your eye data, scores your mental fatigue every 1.5 seconds, and renders it as a living, color-shifting dashboard floating in 3D space. No controllers. No buttons. Just your gaze.

Built to mirror the core interaction philosophy of **NeuralPort's ZEN EYE Pro** — measuring the human mind through the eyes.

---

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black?logo=unity&logoColor=white)
![Python](https://img.shields.io/badge/Python-3.11-3776AB?logo=python&logoColor=white)
![FastAPI](https://img.shields.io/badge/FastAPI-0.100+-009688?logo=fastapi&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-active-brightgreen)

---

## What does it actually do?

Your eyes never lie. Pupil size shrinks when you're mentally exhausted. Blink patterns slow when your brain is overloaded. NeuroVR Dashboard captures these signals at 60fps and translates them into a score — a single number between 0.0 and 1.0 that tells you how fatigued your brain is, right now, inside VR.

```
0.0 ──────────────────────────── 1.0
 🟢 Fresh        🟡 Moderate        🔴 Fatigued
```

The dashboard panel in your VR environment reflects this score live — glowing green when you're sharp, bleeding into red as fatigue builds. And every interaction — every button press, every menu selection — happens through your gaze alone. Stare at something for two seconds. That's a click.

---

## System architecture

```
                    ┌─────────────────────────────┐
                    │      Unity VR Application    │
                    │                              │
  Eye data (60fps)  │  ┌──────────────────────┐   │
  ─────────────────►│  │  GazeInputModule.cs  │   │
                    │  │  pupil · blink · ray  │   │
                    │  └──────────┬───────────┘   │
                    │             │                │
                    │  ┌──────────▼───────────┐   │
                    │  │  WebSocketClient.cs  │   │  POST /fatigue
                    │  │  polls every 1.5s    │───┼──────────────►  Python Backend
                    │  └──────────┬───────────┘   │◄──────────────  fatigue score
                    │             │                │
                    │  ┌──────────▼───────────┐   │
                    │  │ FatigueDashboard.cs  │   │
                    │  │  live color panel    │   │
                    │  └──────────────────────┘   │
                    │                              │
                    │  ┌──────────────────────┐   │
                    │  │   DwellButton.cs     │   │
                    │  │  gaze = input        │   │
                    │  └──────────────────────┘   │
                    └─────────────────────────────┘

                    ┌─────────────────────────────┐
                    │     Python Backend           │
                    │  FastAPI + SQLite            │
                    │  fatigue algorithm           │
                    │  session rolling average     │
                    └─────────────────────────────┘
```

---

## Features

### Real-time fatigue streaming
The Unity client sends eye data to the backend every 1.5 seconds. The backend scores it and sends back a fatigue value. The dashboard smoothly interpolates between scores using `Mathf.Lerp` — no jarring jumps, just a living readout of your mental state.

### Gaze-driven UI — no hands needed
Every button in the scene is a `DwellButton`. Hover your gaze over it for 2 seconds and it activates. A fill bar grows beneath the button as you stare, giving you visual feedback on your progress. The moment it completes — it fires. This is the exact interaction model needed for eye-only VR interfaces like ZEN EYE Pro.

### Adaptive color intelligence
The dashboard doesn't just show a number. It communicates urgency through color:

| Score | Color | Status |
|-------|-------|--------|
| 0.0 – 0.3 | 🟢 Green | FOCUSED |
| 0.3 – 0.6 | 🟡 Yellow | MODERATE |
| 0.6 – 1.0 | 🔴 Red | FATIGUED |

The panel background itself shifts tint subtly as fatigue rises — an ambient signal that doesn't demand your attention but is always there.

### Plug-and-play eye tracking
The entire eye-tracking layer lives in one file: `GazeInputModule.cs`. The mock data produces realistic gaze noise, blink cycles, and pupil oscillation. To use a real headset, replace three lines:

```csharp
// PICO 4 Enterprise
PXR_EyeTracking.GetCombineEyeGazePoint(out gazePoint);

// Tobii
TobiiAPI.GetGazePoint();

// SRanipal (HTC Vive)
SRanipal_Eye_API.GetEyeData(ref eyeData);
```

Everything else — the backend, the dashboard, the dwell buttons — stays exactly the same.

### Programmatic scene construction
There is no manual Unity editor setup. `SceneBuilder.cs` constructs the entire VR environment at runtime: the dashboard panel, the fatigue bar, the TextMeshPro labels, and all three dwell buttons. Hit Play on an empty scene and it builds itself.

---

## Project structure

```
NeuroVR-Dashboard/
│
├── Assets/
│   └── Scripts/
│       ├── Gaze/
│       │   ├── GazeInputModule.cs       # Eye data source (mock / real SDK)
│       │   └── DwellButton.cs           # Gaze-activated button with fill bar
│       │
│       ├── UI/
│       │   ├── FatigueDashboard.cs      # Live panel — score, color, status
│       │   └── SceneBuilder.cs          # Builds entire scene at runtime
│       │
│       ├── Networking/
│       │   └── WebSocketClient.cs       # REST polling client → Python backend
│       │
│       └── Data/
│           └── FatigueData.cs           # Shared structs and response models
│
├── python-backend-2/
│   ├── main.py                          # FastAPI server — fatigue scoring + SQLite
│   └── tests/
│       └── test_api.py                  # pytest test suite
│
├── .github/
│   └── workflows/
│       └── ci.yml                       # GitHub Actions — runs on every push
│
├── .gitignore
└── README.md
```

---

## Getting started

### Prerequisites
- Unity 2022.3 LTS
- Unity packages: XR Interaction Toolkit, OpenXR Plugin, TextMeshPro
- Python 3.11+

### Step 1 — Start the backend

```bash
cd python-backend-2
python -m venv venv

# Windows
venv\Scripts\activate

# Mac / Linux
source venv/bin/activate

pip install fastapi uvicorn sqlalchemy pydantic
uvicorn main:app --reload --port 8001
```

Visit `http://localhost:8001/health` — you should see `{"status":"ok"}`.

### Step 2 — Set up Unity

1. Open the project in Unity Hub (Unity 2022.3 LTS)
2. Install via Package Manager: XR Interaction Toolkit, OpenXR Plugin, TextMeshPro
3. Open `Assets/Scenes/SampleScene`
4. Delete the default Main Camera
5. Right click Hierarchy → **XR → XR Origin (VR)**
6. Right click Hierarchy → **Create Empty** → name it `SystemManager`
7. Add these components to `SystemManager`:
   - `GazeInputModule`
   - `WebSocketClient`
   - `SceneBuilder`
8. Hit **Play**

The scene builds itself. The dashboard appears. Every 1.5 seconds you will see:

```
[WS] Fatigue updated: 0.34 | Player: a3f2b1c0
```

### Step 3 — Interact with gaze

Look at any of the three floating buttons (RESET, PROFILE, HISTORY). Hold your gaze for 2 seconds — the fill bar grows beneath it. When it completes, the button fires and logs to Console.

---

## The fatigue algorithm

The scoring model is a weighted combination of two eye signals:

```
fatigue_score = (pupil_score × 0.7) + (blink_score × 0.3)

pupil_score  = 1.0 - clamp(pupil_diameter, 0, 1)
               # Smaller pupil → higher fatigue

blink_score  = 0.8 if currently blinking else 0.2
               # Blinking mid-task signals cognitive load
```

The backend tracks a rolling session average of both pupil diameter and fatigue score — so you can see how a session evolves over time via `GET /session/{session_id}`.

---

## API reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/fatigue` | Submit eye reading, get fatigue score |
| `GET` | `/session/{id}` | Get session rolling averages |
| `GET` | `/health` | Health check |

**POST `/fatigue` — request body:**
```json
{
  "pupil_diameter": 0.65,
  "is_blinking": false,
  "timestamp_ms": 1700000000000
}
```

**Response:**
```json
{
  "player_id": "a3f2b1c0",
  "fatigue_score": 0.34,
  "pupil_diameter": 0.65,
  "timestamp": "2025-03-15T10:30:00+00:00"
}
```

---

## Roadmap

- [ ] Replace REST polling with true WebSocket for sub-100ms latency
- [ ] Multi-user session switching via gaze selection
- [ ] Gaze heatmap overlay on world-space objects
- [ ] Real eye-tracking SDK integration (PICO 4 / Tobii / SRanipal)
- [ ] Fatigue trend graph rendered in VR as a 3D line chart
- [ ] Export session data as JSON for offline analysis

---

## Connection to NeuralPort's ZEN EYE Pro

This project was built specifically to mirror the engineering challenges of ZEN EYE Pro:

| ZEN EYE Pro | NeuroVR Dashboard |
|-------------|-------------------|
| Measures brain fatigue from eye data | Scores fatigue from pupil + blink signals |
| 1-minute measurement session | 5-second rolling calibration window |
| VR-based measurement environment | Unity OpenXR VR environment |
| Player identification | Session-based player tracking |
| Gaze interaction design | Dwell-time gaze buttons |
| Real-time score feedback | Live color-shifting dashboard panel |

---

## Built by

**Aman Kumar Singh**
3rd Year B.Tech, Material Science and Engineering — IIT Kanpur
Codeforces Specialist (peak 1582) · 400+ problems solved

[LinkedIn](https://linkedin.com/in/aman-singh-iitkanpur) &nbsp;·&nbsp; [GitHub](https://github.com/Amankumarsingh23) &nbsp;·&nbsp; [GazeID VR](https://github.com/Amankumarsingh23/GazeID-VR)

---

*"We are seeking builders who want to push the boundary of what VR can do for the human mind."*
*— NeuralPort*
