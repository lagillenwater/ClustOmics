# load the mlr package
library(mlr)
library(clue)

inputFileName = "IRIS.csv"
outputFileName = "clustering.csv"
k=3

# the first row is the identification row
data = read.csv(inputFileName) 

# load data
feature.count = ncol(data)
ids = data[1]
values = data[2:feature.count]

# Generate the task
kmeans.task = makeClusterTask(data = values)

# Generate the learner
lrn = makeLearner("cluster.kmeans", centers = k)

# Train the learner
model = train(lrn, kmeans.task)
model

# merge ids and labes
labels = model$learner.model$cluster
result = cbind(ids, labels)
result

#save to file
write.csv(x = result ,file = outputFileName, row.names = FALSE, quote = FALSE)
