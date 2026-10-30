import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_csv("results_time.csv", sep=";")

plt.figure(figsize=(9, 5))

plt.plot(
    df["Jobs"],
    df["DECR.1"],
    label="decreasing, alpha = 1.0",
    color="#0004FF",
    marker="o",
)

plt.plot(
    df["Jobs"],
    df["DECR.1.2"],
    label="decreasing, alpha = 1.2",
    color="#0004FF79",
    marker="o",
)

plt.plot(
    df["Jobs"],
    df["DECR.1.5"],
    label="decreasing, alpha = 1.5",
    color="#0004FF38",
    marker="o",
)


plt.plot(
    df["Jobs"],
    df["INCR.1"],
    label="increasing, alpha =  1.0",
    color="#FF0000",
    marker="o",
)

plt.plot(
    df["Jobs"],
    df["INCR.1.2"],
    label="increasing, alpha =  1.2",
    color="#FF00008C",
    marker="o",
)

plt.plot(
    df["Jobs"],
    df["INCR.1.5"],
    label="increasing, alpha =  1.5",
    color="#FF00002C",
    marker="o",
)

plt.yscale("log")
plt.xlabel("number of jobs n")
plt.ylabel("runtime in ms (log scale)")
plt.grid()
plt.legend()

plt.tight_layout()
plt.savefig("offline_v1.pdf")
plt.show()