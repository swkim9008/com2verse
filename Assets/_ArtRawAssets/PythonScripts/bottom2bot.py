import UnityEngine
import UnityEditor
import os


def test_drive():
    db = UnityEditor.AssetDatabase
    prefabs = db.FindAssets(".prefab")
    print(len(prefabs))


def apply_to(root):
    for root, dirs, files in os.walk(root):
        for f in files:
            print(f)
            if "_BOTTOM_" not in f:
                continue

            src = f"{root}/{f}"
            dst = path.replace("_BOTTOM_", "_BOT_")
            os.rename(src, dst)
            print(f"{src} -> {dst}")


def main():
    test_drive()
    # apply_to("Assets/Project/Bundles/03_Avatar/CH_pack/PC01_M")
    # apply_to("Assets/Project/Bundles/03_Avatar/CH_pack/PC01_W")


main()
