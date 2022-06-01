#!/bin/bash
echo "Waiting for Pipeline Service at address ${BASE_URL_PIPELINE_SERVICE}/health, attempting every 5s"
sleep 5
until $(curl --silent --fail "${BASE_URL_PIPELINE_SERVICE}"/health | grep "Healthy" > /dev/null); do
    printf '.'
    sleep 5
done
echo " Success: Reached Pipeline Service"

echo "Waiting for Learning Service at address ${BASE_URL_PIPELINE_SERVICE}/health, attempting every 5s"
sleep 5
until $(curl --silent --fail "${BASE_URL_LEARNING_SERVICE}"/health | grep "OK" > /dev/null); do
    printf '.'
    sleep 5
done
echo " Success: Reached Learning Service"
