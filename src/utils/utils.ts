export const secondsToDate = (seconds: number) => {
  const normalDate = new Date(seconds * 1000).toLocaleString("en-US", {
    timeZone: "UTC",
  });

  return normalDate;
};
