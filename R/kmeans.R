# # load the mlr package
# #install.packages('mlr')
# #install.packages("clue")

# library(mlr)
# library(clue)

# input.file.name = "iris.csv"
# output.file.name = "clustering.csv"
# k=3

# # the first row is the identification row
# data = read.csv(input.file.name) 

# # load data
# feature.count = ncol(data)
# ids = data[1]
# values = data[2:feature.count]

# # Generate the task
# kmeans.task = makeClusterTask(data = values)

# # Generate the learner
# lrn = makeLearner("cluster.kmeans", centers = k)

# # Train the learner
# model = train(lrn, kmeans.task)
# model

# # merge ids and labes
# labels = model$learner.model$cluster
# result = cbind(ids, labels)
# result

# #save to file
# write.csv(x = result ,file = output.file.name, row.names = FALSE, quote = FALSE)
