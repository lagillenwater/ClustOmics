inputFileName = "iris.csv"
outputFileName = "clustering.csv"
k = 3 # number of clusters
d = 2 # average dimnesions

# the first row is the identification row
data = read.csv(inputFileName) 

# load data
feature.count = ncol(data)
ids = data[1]
values = data[2:feature.count]

# clustering
temp.clustering = ProClus(values, k, d)

label = 1 
for (cluster in clustering)
{
  ids = cluster$objects
  labels =  rep(c(label), times = length(ids))
  clustering = cbind(ids, label)
  ids = ids + 1
}
