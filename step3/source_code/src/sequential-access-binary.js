export default class SequentialAccessBinary {
  constructor(uint8Array = void(0)) {
    if (typeof(uint8Array) === "undefined") {
      this.view = new DataView(new ArrayBuffer(4096));
      this.offset = 0;
    } else {
      this.view = new DataView(uint8Array.buffer, uint8Array.byteOffset, uint8Array.byteLength);
      this.offset = 0;
    }
  }

  _extend() {
    const newBuffer = new ArrayBuffer(this.view.byteLength * 2);
    new Uint8Array(newBuffer).set(new Uint8Array(this.view.buffer, this.view.byteOffset, this.view.byteLength));
    this.view = new DataView(newBuffer);
  }

  _readValue(byteLength, methodName) {
    const value = this.view[methodName](this.offset, true);
    this.offset += byteLength;
    return value;
  }
  _writeValue(value, byteLength, methodName) {
    while (this.offset + byteLength > this.view.byteLength) this._extend();
    this.view[methodName](this.offset, value, true);
    this.offset += byteLength;
  }

  readUint8() {
    return this._readValue(1, "getUint8");
  }
  writeUint8(value) {
    this._writeValue(value, 1, "setUint8");
  }
  readUint8Array(arrayLength) {
    return new Array(arrayLength).fill().map(() => this.readUint8());
  }
  writeUint8Array(values) {
    values.forEach(value => {
      this.writeUint8(value);
    });
  }

  readInt8() {
    return this._readValue(1, "getInt8");
  }
  writeInt8(value) {
    this._writeValue(value, 1, "setInt8");
  }
  readInt8Array(arrayLength) {
    return new Array(arrayLength).fill().map(() => this.readInt8());
  }
  writeInt8Array(values) {
    values.forEach(value => {
      this.writeInt8(value);
    });
  }

  readUint16() {
    return this._readValue(2, "getUint16");
  }
  writeUint16(value) {
    this._writeValue(value, 2, "setUint16");
  }
  readUint16Array(arrayLength) {
    return new Array(arrayLength).fill().map(() => this.readUint16());
  }
  writeUint16Array(values) {
    values.forEach(value => {
      this.writeUint16(value);
    });
  }

  readInt16() {
    return this._readValue(2, "getInt16");
  }
  writeInt16(value) {
    this._writeValue(value, 2, "setInt16");
  }
  readInt16Array(arrayLength) {
    return new Array(arrayLength).fill().map(() => this.readInt16());
  }
  writeInt16Array(values) {
    values.forEach(value => {
      this.writeInt16(value);
    });
  }

  readUint32() {
    return this._readValue(4, "getUint32");
  }
  writeUint32(value) {
    this._writeValue(value, 4, "setUint32");
  }
  readUint32Array(arrayLength) {
    return new Array(arrayLength).fill().map(() => this.readUint32());
  }
  writeUint32Array(values) {
    values.forEach(value => {
      this.writeUint32(value);
    });
  }

  readInt32() {
    return this._readValue(4, "getInt32");
  }
  writeInt32(value) {
    this._writeValue(value, 4, "setInt32");
  }
  readInt32Array(arrayLength) {
    return new Array(arrayLength).fill().map(() => this.readInt32());
  }
  writeInt32Array(values) {
    values.forEach(value => {
      this.writeInt32(value);
    });
  }

  readFloat32() {
    return this._readValue(4, "getFloat32");
  }
  writeFloat32(value) {
    this._writeValue(value, 4, "setFloat32");
  }
  readFloat32Array(arrayLength) {
    return new Array(arrayLength).fill().map(() => this.readFloat32());
  }
  writeFloat32Array(values) {
    values.forEach(value => {
      this.writeFloat32(value);
    });
  }

  readString(byteLength, encoding) {
    return new TextDecoder(encoding).decode(new Uint8Array(this.readUint8Array(byteLength)));
  }
  writeString(value, encoding) {
    this.writeUint8Array(new TextEncoder(encoding, {NONSTANDARD_allowLegacyEncoding: true}).encode(value));
  }

  readNullTerminatedString(byteLength, encoding) {
    return new TextDecoder(encoding).decode(new Uint8Array(_.takeWhile(this.readUint8Array(byteLength), x => x > 0)));
  }
  writeNullTerminatedString(value, byteLength, encoding) {
    const previousOffset = this.offset;
    this.writeString(value, encoding);
    _.range(this.offset - previousOffset, byteLength).forEach(() => {
      this.writeUint8(0);
    });
  }

  toUint8Array() {
    return new Uint8Array(this.view.buffer, this.view.byteOffset, this.offset);
  }
}
