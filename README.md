https://excalidraw.com/#json=-eCMybhYoN8Ip9liNq6eA,0z2f2LAhDFGPhSBAVAc4Jg

# BankoApi

A `.env` file is needed to connect to the database.
You can find a `.envTemplate` to know which variables are required.

## Logger

All log output follows the format:

```
[2026-06-22 14:30:00] ERROR: ❌ human readable message
    Stack: Exception details
```

Each log level is prefixed with an emoji for quick visual scanning:

| Level | Emoji | Meaning |
|---|---|---|
| `Critical` | 💥 | System is broken, immediate attention needed |
| `Error` | ❌ | An operation failed |
| `Warning` | ⚠️ | Something unexpected occurred |
| `Information` | ℹ️ | General application info |
| `Debug` | 🔍 | Detailed diagnostics for development |
| `Trace` | 🔬 | Verbose internal tracing |
