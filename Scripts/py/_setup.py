import sys
import os
from pathlib import Path


dirpath = Path(os.path.dirname(__name__)).absolute()


class SymlinkPair:
    srd = None
    dst = None

    def __init__(self, src, dst):
        super().__init__()
        self.src = (Path(dirpath) / Path(src)).resolve()
        self.dst = (Path(dirpath) / Path(dst)).resolve()

    def make(self, is_dir=True):
        if os.path.exists(self.dst):
            print("symlink already exists")
            return

        os.symlink(self.src, self.dst, is_dir)


portals = []

if not os.path.isdir("./portals"):
    print("creating portals dir..")
    os.makedirs("./portals")

else:
    portals = os.listdir("./portals")

SymlinkPair("../Assets/Project/Bundles/100001/CH_ArtAsset/SHARED", "./portals/CH_SHARED_ASSETS").make(True)
SymlinkPair("../Assets/Project/Bundles/100001/BG_ArtAsset/SHARED", "./portals/BG_SHARED_ASSETS").make(True)


print(sys.version)
print(sys.executable)
os.system("python --version")
os.system("python -m pip install pipenv")
os.system("pipenv --python 3.10")
os.system("pipenv install -r requirements.txt")
