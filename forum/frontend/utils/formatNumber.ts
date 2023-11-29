export default (num: number): string => {
  if (num < 1000) {
    return num.toString(); // If the number is less than 1000, return it as a string
  } else if (num < 1000000) {
    return (num / 1000).toFixed(1).replace(/\.0$/, '') + 'K'; // For thousands
  } else if (num < 1000000000) {
    return (num / 1000000).toFixed(1).replace(/\.0$/, '') + 'M'; // For millions
  } else if (num < 1000000000000) {
    return (num / 1000000000).toFixed(1).replace(/\.0$/, '') + 'B'; // For billions
  } else {
    return (num / 1000000000000).toFixed(1).replace(/\.0$/, '') + 'T'; // For trillions
  }
};
