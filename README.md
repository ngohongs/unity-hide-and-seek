# Hide and Seek Dark Mode

A 3D hide and seek game set in a dark room where players must catch an AI agent as quickly as possible. Features custom pixel art rendering effects and intelligent enemy AI behavior.

https://github.com/user-attachments/assets/5e0ed02e-b696-4009-be32-ab58d2d2d3aa

## 🎮 Game Overview

Hide and seek but in a dark room - the player tries to catch the agent as fast as possible. Each catch time is stored in the leaderboard. To catch the agent, the player needs to collide with them.

## 🎯 Target Platform

- **Primary**: PC
- **Secondary**: Android support
- Gameplay experience remains consistent across platforms with only control differences

## 🔧 Technical Features

### Core Systems
- **3D Environment**: Built using ProBuilder with ProGrids for world creation
- **Cross-Platform UI**: Implemented with UI Toolkit for both PC and Android
- **Modern Input System**: All inputs (except Android joystick) processed through Unity's Input System
- **Smart AI**: Non-trivial AI agent that intelligently hides behind walls and relocates when detected

### Custom Rendering Pipeline
- **Pixelation Effect**: Custom post-processing effect for authentic pixel art look
- **Edge Detection**: Implemented using Sobel convolution kernel in HLSL
- **Color Banding**: Creates jagged color gradients for retro aesthetic
- **Custom Shaders**: All effects coded in HLSL, located in `Assets/Shaders/Pixelate/`

### Particle Systems
- Spark simulation effects for enhanced visual feedback
- Light flash effects simulating electrical sparks

## 📁 Project Structure

```
Assets/
├── UI/                     # UI Toolkit template files
├── Settings/               # Render pipeline configuration
├── Shaders/Pixelate/       # Custom HLSL shaders
├── Renderer Features/      # Custom render features
└── [Input Map Location]    # Input system configuration
```

## 🛠️ Plugins & Dependencies

- **ProBuilder with ProGrids**: World creation and level design
- **UI Toolkit**: Cross-platform user interface
- **Input System**: Modern input handling

## ⚙️ Editor Extensions

### 1. Pixelation Render Feature
- **Location**: `Assets/Settings/Pixelate Render Pipeline Asset_Renderer`
- **Controls**:
  - Pixel size adjustment
  - Geometry outline settings
  - Color banding intensity

### 2. Light Flash System (`LightFlash.cs`)
- Fine-tune spark flash light effects
- Customizable intensity and duration

### 3. Enemy AI System
- **EnemyMovement.cs**: Controls hiding behavior and sensitivity
- **OnRangeChecker.cs**: Handles line-of-sight detection from enemy perspective

## 🎨 Rendering Features

The game extensively covers advanced rendering techniques:

- **Post-Processing Pipeline**: Custom pixelation effect applied as renderer feature
- **Shader Programming**: HLSL shaders for edge detection and color effects
- **Visual Style**: Authentic pixel art aesthetic in 3D space

## 🎮 Controls

- Input mapping configured through Unity's Input System
- Runtime keybinding support (PC)
- Touch controls for Android version

## 🏆 Leaderboard System

- Tracks catch times for each successful agent capture
- Encourages replayability and competitive play

## 🔮 Future Improvements

- Enhanced pixel art rendering robustness
- More flexible visual effects system
- Expanded customization options

## 💡 Development Notes

### Enjoyed Most
Replicating authentic pixel art aesthetics in 3D space, particularly the custom rendering pipeline implementation.

### Main Challenges
- Runtime keybinding system implementation
- Cross-platform UI scaling for PC and Android screens

### Recommendations for Future Developers
- **Allocate sufficient development time**
- Focus on rendering pipeline early in development
- Test UI scaling across different screen sizes regularly

## 🚀 Getting Started

1. Clone the repository
2. Open in Unity (compatible version recommended)
3. Configure render pipeline settings in `Assets/Settings/`
4. Build for target platform (PC/Android)

## 📝 Additional Requirements Met

- ✅ Particle Systems
- ✅ 3D Game Environment  
- ✅ Smart Non-trivial AI
- ✅ Custom Rendering Pipeline
- ✅ Cross-platform Support

---

*This project demonstrates advanced Unity development techniques including custom shaders, AI behavior, and cross-platform deployment.*
