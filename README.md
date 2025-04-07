# 🧙 Dev Panel Wizard

**Dev Panel Wizard** is a modular in-editor utility for Unity designed to simplify debugging, testing, and tweaking your game in real-time.  
Create customizable developer tabs to expose runtime variables and invoke methods from your MonoBehaviour scripts — without writing extra UI code!

---

## 📦 Installation

### 🔗 Unity Package Manager (UPM)

You can install this plugin using Git URL:

1. Open `Edit > Project Settings > Package Manager`
2. Add the following Git URL to your project:

```
https://github.com/hendrakid/com-whisper-devpanelwizard.git
```

> ✅ Make sure Git is installed on your system.

---

## 🧪 Usage

### 🚀 Opening the Dev Panel

- Go to `Window > Dev Tools > Dev Panel Wizard`

### ➕ Adding Tabs

1. Click the **Add Tab** button
2. Select a tab type (e.g., `ActionInvoker`)
3. Add Script with MonoBehaviour you want to Interact
4. Interact with variables and methods directly from the panel

---

### 🧷 Attributes

#### 🔹 `[ExposeVariable]` – Show variables

Use this to expose fields in the panel and edit them live.

```csharp
[ExposeVariable]
public int speed;
```

✅ **Supported types:**

- Basic types: `int`, `float`, `bool`, `string`
- Vectors: `Vector2`, `Vector3`, `Vector4`
- Colors: `Color`, `Color32`
- Enums, `LayerMask`
- Unity objects like `GameObject`, `Transform`, etc.

---

#### 🔸 `[ActionButton]` – Call methods

Use this to expose methods as clickable buttons in the panel.

```csharp
[ActionButton]
public void Dash()
{
    Debug.Log("Player dashed!");
}
```

✅ **Methods can:**

- Have **no parameters**
- Have **parameters** (primitives, strings, Unity types)

```csharp
[ActionButton]
public void Heal(int amount)
{
    Debug.Log($"Healed {amount} HP!");
}
```

🔄 Parameters will appear as editable fields before the button.

---

### 🔧 Settings Location

🛠 You can manage tab settings under:

```
Assets/Plugins/DevPanelWizard/Settings
```

Tab configurations are saved as ScriptableObjects for persistence.

---

## ✨ Example

```csharp
[ExposeVariable]
public float moveSpeed;

[ActionButton]
public void Teleport(Vector3 position)
{
    transform.position = position;
}
```

---

## 🙏 Credits

- Developed by [@hendrakid](https://github.com/hendrakid)
- Special thanks to Unity’s extensibility system and all devs who inspired this workflow

---

## 📫 Contact Us

Have suggestions, issues, or want to contribute?

- 📬 [Open an Issue](https://github.com/hendrakid/com-whisper-devpanelwizard/issues)
- 📧 your.email@example.com *(replace with your actual contact)*

---

## 📄 License

MIT License — free to use, modify, and distribute.
