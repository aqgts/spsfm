_.mixin({
  rangeClosed(arg1, arg2, arg3) {
    switch (arguments.length) {
    case 1:
      return _.range(arg1 + 1);
    case 2:
      return _.range(arg1, arg2 + 1);
    case 3:
      return _.range(arg1, arg2 + 1, arg3);
    }
  },
  prod(array) {
    return _.prodBy(array, _.identity);
  },
  prodBy(array, iteratee) {
    const iterateeFunc = _.iteratee(iteratee);
    return array.reduce((prod, element) => prod * iterateeFunc(element), 1);
  }
}, {chain: false});
