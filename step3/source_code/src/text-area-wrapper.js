class NullTextArea {
  constructor(value = "") {
    this.value = value;
    this.scrollTop = 0;
    this.scrollHeight = 0;
  }
}

export default class TextAreaWrapper {
  constructor(textArea = new NullTextArea()) {
    this.textArea = textArea;
    this._lastModified = new Date().getTime();
  }
  _wait(resolve) {
    const previous = this._lastModified;
    const now = new Date().getTime();
    if (now - previous <= 100) {
      resolve();
    } else {
      this._lastModified = now;
      setTimeout(resolve, 0);
    }
  }
  clear() {
    this.textArea.value = "";
  }
  clearAsync() {
    return new Promise((resolve, reject) => {
      this.clear();
      this._wait(resolve);
    });
  }
  append(message) {
    this.textArea.value += message + "\n";
    this.textArea.scrollTop = this.textArea.scrollHeight;
  }
  appendAsync(message) {
    return new Promise((resolve, reject) => {
      this.append(message);
      this._wait(resolve);
    });
  }
  update(message) {
    const lines = this.textArea.value.split(/\n/).slice(0, -1);
    if (lines[lines.length - 1] === message) return false;
    this.textArea.value = lines.slice(0, -1).map(line => line + "\n").join("") + message + "\n";
    this.textArea.scrollTop = this.textArea.scrollHeight;
    return true;
  }
  updateAsync(message) {
    return new Promise((resolve, reject) => {
      if (this.update(message)) this._wait(resolve);
      else resolve();
    });
  }
  progress(message, count) {
    if (arguments.length === 2) {
      this.registeredMessage = message;
      this.registeredCount = count;
      this.registeredIndex = 0;
      this.append(`${message}(0.0%)`);
      return true;
    } else if (arguments.length === 0) {
      this.registeredIndex++;
      const progressRate = Math.floor(this.registeredIndex / this.registeredCount * 1000) / 10;
      return this.update(`${this.registeredMessage}(${Number.isInteger(progressRate) ? `${progressRate}.0` : progressRate}%)`);
    }
  }
  progressAsync(...args) {
    return new Promise((resolve, reject) => {
      if (this.progress(...args)) this._wait(resolve);
      else resolve();
    });
  }
}
