import csv
import requests
import sys


def GetHistoricalData(symbol, interval1, interval2, count):
    print("https://api.binance.com/api/v3/klines?symbol=" + symbol + "&interval=" + interval1 + interval2 + "&limit=" + count)
    response = requests.get(
        "https://api.binance.com/api/v3/klines?symbol=" + symbol + "&interval=" + interval1 + interval2 + "&limit=" + count)
    data = response.json()
    with open('output.csv', 'w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerows(data)


GetHistoricalData(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4])
