ws C#
The main purposes of this project are the following:
- Via an API request get all symbols from Binance
- Start reading on a predetermined amount of websockets the data of these symbols (I needed a limitation as my home network can't handle 1000+ websockets)
- Store the read in data in a MongoDB database

So to sum it all up, this project creates local orderbooks for many symbols and keeps them up to date with adding about 30ms latency on my side (thanks to MongoDB)
