#install.packages("subspace")
library(subspace)

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

result = data.frame(matrix(ncol = 2, nrow = 0))


for (cluster.id in (1:length(temp.clustering)))
{
  sids = temp.clustering[[cluster.id]][["objects"]]
  labels = rep(cluster.id, length(sids))
  cluster = cbind(sids, labels)
  result = rbind(result, cluster)
}

#save to file
write.csv(x = result ,file = outputFileName, row.names = FALSE, quote = FALSE)
