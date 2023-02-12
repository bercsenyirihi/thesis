# Thesis
This is a repository for showing the results of my thesis.

The main goal in my thesis was to create a framework, that can help with the
volatility analysis of cryptocurrencies supported by a local database, as well as
demonstrating it through a working, visual example.
I divided the task into 3 subtasks, the first two together form the basis of the
framework, while the last one merely demonstrates the achieved results. In the first part,
I first read in the cryptocurrency pairs existing in the system, from which the data can be
read. Then I replicate the current state of the corresponding tables, and after that I
periodically update the locally stored data by querying the changes. The last step is to
store the data in a local database. The second step is the sorting of the already stored data
into a high-performance, in-memory database, from where data can be easily and quickly
be sorted and read based on various aspects during runtime. The last part was the user
interface, which displays the data requested by the user through graphs visually, and close
to real time.
In addition, my program was given a second function: it can visually communicate
historycal data to the user, as well as perform statistical analysis on these data sets. For
this, I saved the historycal data to a file via a script, and then performed an analysis on its
contents
