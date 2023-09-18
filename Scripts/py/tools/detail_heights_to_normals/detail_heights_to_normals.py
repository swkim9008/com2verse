from pathlib import Path

import numpy as np
import moderngl as mg
import imageio.v3 as ii
from glm import vec4
from psd_tools import PSDImage
from psd_tools.api.layers import Group


_gl = None
_program = None
_quad = None


def get_compute():
    return open(Path("./tools/detail_heights_to_normals/") / "./gl/heights_into_normals_palette.glsl").read()


def get_gl():
    global _gl
    if not _gl:
        _gl = mg.create_context(require=460, standalone=True)
    return _gl


def get_program():
    global _program
    if not _program:
        gl = get_gl()
        _program = gl.compute_shader(get_compute())
    return _program


def run_compute_shader(input_buffer_xy, input_buffer_zw, output_buffer, size):
    input_buffer_xy.bind_to_storage_buffer(0)
    input_buffer_zw.bind_to_storage_buffer(1)
    output_buffer.bind_to_storage_buffer(4)
    program = get_program()
    program["u_resolution"].value = (size[0], size[1])
    program.run(size[0] // 8, size[1] // 8)


def buffer_to_nparray(buffer, size):
    output = np.frombuffer(buffer.read(), dtype=np.float32)
    shape = (size[1], size[0], size[2])
    output = np.reshape(output, shape).astype(np.uint8)
    return output


def confirm_formats(image):
    if image.shape[2] == 3:
        canvas = np.zeros((image.shape[0], image.shape[1], 4))
        canvas[:, :, :3] = image[:, :, :]
        image = canvas

    return image.astype(np.float32)


def dump_image(size, xy_img, zw_img):
    width, height = size

    gl = get_gl()

    xy_img = confirm_formats(xy_img)
    zw_img = confirm_formats(zw_img)

    input_buffer_xy = gl.buffer(xy_img)
    input_buffer_zw = gl.buffer(zw_img)
    output_buffer = gl.buffer(reserve=width * height * 4 * 4)
    run_compute_shader(
        input_buffer_xy,
        input_buffer_zw,
        output_buffer,
        (width, height, 4)
    )

    img = buffer_to_nparray(output_buffer, (height, width, 4))

    input_buffer_xy.release()
    input_buffer_zw.release()
    output_buffer.release()

    ii.imwrite(Path("./portals/CH_SHARED_ASSETS/Texture") / "./DETAIL_NORMALS.tga", img)


def dump_psd(psd_path):
    psd_img = PSDImage.open(psd_path)

    width, height = psd_img.width, psd_img.height
    xy_layer = next((layer for layer in psd_img if layer.name == "XY"), None)
    zw_layer = next((layer for layer in psd_img if layer.name == "ZW"), None)

    xy_img = np.array(xy_layer.composite((0, 0, width, height)))
    zw_img = np.array(zw_layer.composite((0, 0, width, height)))
    
    dump_image((width, height), xy_img, zw_img)


def main():
    dump_psd(Path("./portals/CH_SHARED_ASSETS/Texture") / "./DETAIL_HEIGHTS.psd")


if __name__ == "__main__":
    main()
