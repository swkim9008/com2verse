import sys

from PySide6 import QtCore, QtWidgets


class Client(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()

        layout = QtWidgets.QVBoxLayout(self)

        label = QtWidgets.QLabel("RGB to HSV")
        layout.addWidget(label)

        button = QtWidgets.QPushButton("Convert!")
        layout.addWidget(button)


def main():
    app = QtWidgets.QApplication(sys.argv)
    widget = Client()
    widget.show()
    sys.exit(app.exec())


if __name__ == "__main__":
    main()
