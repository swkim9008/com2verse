import random

import UnityEngine as u
import UnityEditor as ue
from Com2Verse.CustomizeLayer.Runtime.NpcAvatarLayer import NpcAvatarLayer
from Com2Verse.RenderFeatures.Data import Avatar3AssemblerContextAsset as AvatarContext


container = u.GameObject("Container")
top_count = 101
bot_count = 85
hair_count = 20
hat_count = 21
loop_count = min(top_count * bot_count, 2048)
loop = 0
for bot in range(0, bot_count):
    for top in range(0, top_count):
        ue.EditorUtility.DisplayProgressBar("Npc Population", f"populating npc.. {loop} / {loop_count}..", loop / loop_count)
        loop += 1

        npc = u.GameObject(f"NPC_{top}_{bot}")
        npc.transform.position = u.Vector3(top, 0.0, bot)
        npc.transform.SetParent(container.transform)
        npc_layer = npc.AddComponent[NpcAvatarLayer]()

        avatar_context = AvatarContext.Get(None)
        avatar = avatar_context.Avatar

        avatar.SetSel = -1
        avatar.TopSel = top
        avatar.BottomSel = bot
        avatar.HairSel = random.randrange(hair_count)
        avatar.HatSel = random.randrange(hat_count + 1) - 1
        avatar.GlovesSel = -1
        avatar.BagSel = -1
        avatar.GlassSel = -1

        avatar.FacePaintingIndex = -1
        avatar.BodyShapeSel = 2
        avatar.SkinVisibility = True
        avatar.FacelessMode = False

        npc_layer.SetAvatarContextAsset(avatar_context)
        npc_layer.Populate()

        if loop >= loop_count:
            break

    if loop >= loop_count:
        break

ue.EditorUtility.ClearProgressBar()
