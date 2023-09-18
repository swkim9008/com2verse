import os



def apply_to(root):
    for root, dirs, files in os.walk(root):
        for f0 in files:
            if "_BOT_" not in f0:
                continue

            f1 = f0.replace("_BOT_", "_BOTTOM_")
            src = f"{root}/{f0}"
            dst = f"{root}/{f1}"
            os.rename(src, dst)
            print(f"{src} -> {dst}")


def main():
    apply_to("D:/c2vclient_renderfeature/Assets/Project/Bundles/03_Avatar/CH_pack/PC01_M")
    apply_to("D:/c2vclient_renderfeature/Assets/Project/Bundles/03_Avatar/CH_pack/PC01_W")


main()
