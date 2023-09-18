import UnityEngine as u
from Com2Verse.CustomizeLayer import AvatarCustomizeLayer as layer
from Com2VerseEditor.EditorSignals import Signal, ForceUpdateSignal

layer.EnablePooling = not layer.EnablePooling
Signal.Emit[ForceUpdateSignal]()