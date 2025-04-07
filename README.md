# 🧙‍♂️ DevPanelWizard

[![Latest Release](https://img.shields.io/github/v/release/hendrakid/com-whisper-devpanelwizard?label=Download)](https://github.com/hendrakid/com-whisper-devpanelwizard/releases/latest)

A powerful and extensible Unity Editor plugin to expose variables and invoke methods directly from the editor. Perfect for debugging, testing, and gameplay tweaking during development.

---

## 🚀 Features

- 🧪 Expose and edit runtime variables from a custom editor window.
- 🎯 Call any public method on a MonoBehaviour with or without parameters.
- 📁 Modular tab system with ScriptableObject-based persistence.
- 🧩 Plug-and-play with zero setup.

---

## 📦 Installation

There are two ways to install:

### 🔗 Using Git URL (recommended for Unity Package Manager)

    1. Open `Edit > Project Settings > Package Manager`.
    2. Add the following Git URL:
   ```
   https://github.com/hendrakid/com-whisper-devpanelwizard.git
   ```

### 📁 Manual `.unitypackage` Import

    1. Go to the [Releases Page](https://github.com/hendrakid/com-whisper-devpanelwizard/releases/latest).
    2. Download the latest `.unitypackage`.
    3. In Unity: `Assets > Import Package > Custom Package` and select the downloaded file.

---

## ⚙️ Usage

### 🪄 Add the Dev Panel Wizard

Open the editor window via `Tools > Dev Panel Wizard`
### ➕ Adding Tabs

    1. Click the **Add Tab** button
    2. Select a tab type (e.g., `ActionInvoker`)
    3. Add Script with MonoBehaviour you want to Interact
    4. Interact with variables and methods directly from the panel

---

### ✨ Expose Variables

Use `[ExposeVariable]` attribute to expose any field in the custom panel.

```csharp
[ExposeVariable]
public float movementSpeed;
```

You can expose fields of types like:

- `int`, `float`, `double`, `string`, `bool`
- `Vector2`, `Vector3`, `Vector4`
- `Color`, `Quaternion`, `Bounds`
- `Enum`, `LayerMask`, `AnimationCurve`
- Unity objects: `GameObject`, `Transform`, `MonoBehaviour`, etc.

### ⚡ Add Action Buttons

Use `[ActionButton]` attribute to invoke any method—supports parameters or not.

```csharp
[ActionButton]
public void ResetPlayer() {
    Debug.Log("Player reset!");
}

[ActionButton]
public void TeleportTo(Vector3 position) {
    transform.position = position;
}
```

You can pass values directly from the UI when the method has parameters.

---


---

## 🙏 Credit

Created with ❤️ by [@hendrakid](https://github.com/hendrakid)

Special thanks to the Unity community for inspiration and feedback.

---

## 📬 Contact

- 💬 Issues or feature requests? [Open an issue](https://github.com/hendrakid/com-whisper-devpanelwizard/issues)

---

## 📌 License

MIT — feel free to use, modify, and contribute!

