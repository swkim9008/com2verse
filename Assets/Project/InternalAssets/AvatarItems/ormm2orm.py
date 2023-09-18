import os
import imageio.v3 as ii
import numpy as np


def process_ormm(path):
    src = ii.imread(path)

    if src.shape[2] == 4:
        blue = src[:, :, 3]
    elif src.shape[2] == 3:
        blue = np.zeros_like(src[:, :, 0])

    if src.shape[2] == 4:
        alpha = src[:, :, 3]
    elif src.shape[2] == 3:
        alpha = np.zeros_like(src[:, :, 0])

    h, w = src.shape[0], src.shape[1]
    dst_orm = np.zeros(shape=[h, w, 4], dtype=np.uint8)
    dst_orm[:, :, 0: 2] = src[:, :, 0: 2]
    dst_orm[:, :, 2] = blue
    dst_orm[:, :, 3] = alpha

    dst_m = np.zeros(shape=[h, w, 4], dtype=np.uint8)
    dst_m[:, :, 0] = alpha

    os.remove(path)

    dst_orm_path = path.replace("_ORMM.tga", "_ORM.tga")
    dst_m_path = path.replace("_ORMM.tga", "_M.tga")

    ii.imwrite(dst_orm_path, dst_orm)
    ii.imwrite(dst_m_path, dst_m)
    print(path)


def process_meta(path):
    # os.rename(path, path.replace("_ORMM.tga.meta", "_ORM.tga.meta"))
    pass


def main():
    for root, dirs, files in os.walk("./"):
        for filename in files:
            if filename.endswith("_ORMM.tga"):
                process_ormm(f"{root}\\{filename}")
            elif filename.endswith("_ORMM.tga.meta"):
                process_meta(f"{root}\\{filename}")



if __name__ == "__main__":
    main()
