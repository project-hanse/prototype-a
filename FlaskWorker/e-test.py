# https://www.linkedin.com/posts/lexfridman_select-numbers-between-0-and-1-randomly-until-activity-6774714449338150912-SLOS

import random
from math import e
from time import sleep

epochs = range(0, 1000)
iterations = 10
selections = []
estimations = []

for ep in epochs:
    epoch_count = 0
    while epoch_count < iterations:
        select_count = 0
        random_sum = 0
        while random_sum <= 1:
            random_sum += random.random()
            select_count += 1
        selections.append(select_count)
        random_sum = 0
        select_count = 0
        epoch_count += 1

    avg = (sum(selections) / len(selections))
    diff = abs(e - avg)
    estimations.append(avg)
    print("[%i] Average number of selections: %0.4f (%0.4f)" % (ep, avg, e))
    print("[%i] Off by: %0.6f" % (ep, diff))
    sleep(0.01)

print(estimations)
