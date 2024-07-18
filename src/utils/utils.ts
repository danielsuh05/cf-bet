export const secondsToDate = (seconds: number) => {
  const normalDate = new Date(seconds * 1000).toLocaleString("en-US", {
    timeZone: "UTC",
  });

  return normalDate;
};

export const numberWithCommas = (x: number) => {
  return x.toString().replace(/\B(?<!\.\d*)(?=(\d{3})+(?!\d))/g, ",");
};
