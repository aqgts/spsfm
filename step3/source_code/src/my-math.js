export default {
  mod(x, y) {
    return (x % y + y) % y;
  },
  binom(n, k) {
    if (2 * k > n) k = n - k;
    if (k === 0) return 1;
    return _.rangeClosed(n - k + 1, n).reduce((prod, x) => prod * x, 1) / _.rangeClosed(1, k).reduce((prod, x) => prod * x, 1);
  },
  clamp(x, min, max) {
    return [min, x, max].sort((x, y) => x - y)[1];
  },
  lerp(x, y, t) {
    return x + (y - x) * t;
  },
  cartesianProduct(...args) {
    if (typeof(args[args.length - 1]) === "function") {
      const callback = args.pop();
      const callbackArgs = [];
      (function loop() {
        if (args.length === 0) {
          callback(...callbackArgs);
        } else {
          const values = args.pop();
          for (const value of values) {
            callbackArgs.unshift(value);
            loop();
            callbackArgs.shift(value);
          }
          args.push(values);
        }
      })();
    } else {
      return args.reduce((matrix, values) => values.reduce((array, value) => array.concat(matrix.map(row => row.concat([value]))), []), [[]]);
    }
  },
  inverse(f, minDomain, maxDomain) { // fは狭義単調増加
    return function (y) {
      if (y < f(minDomain) || f(maxDomain) < y) throw new Error(`Range error: ${y} is not in [${minDomain}, ${maxDomain}]`);
      let minX = minDomain;
      let maxX = maxDomain;
      let previousAvrX = null;
      let avrX = null;
      while (true) {
        [previousAvrX, avrX] = [avrX, (minX + maxX) / 2];
        if (previousAvrX === avrX) return avrX;
        const avrY = f(avrX);
        if (avrY === y) {
          return avrX;
        } else if (avrY < y) {
          minX = avrX;
        } else {
          maxX = avrX;
        }
      }
    };
  },
  nextPow2(value) {
    if (!Number.isInteger(value) || value < 1) throw new RangeError(`value (${value}) must be positive.`);
    let result = 1; 
    for (let i = value; i >= 1; i /= 2, result *= 2); 
    if (2 * value  === result) result = value;
    return result;
  }
};
