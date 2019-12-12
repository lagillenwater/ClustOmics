# # this file gets a dataset and a clusterign and returns the overall and individual Silhouette scores
# dataset.path = 'iris.csv'
# clustering.path = 'clustering.csv'

# dataset = read.csv(dataset.path)
# col.names = colnames(dataset)
# col.names[1] = 'sid'
# colnames(dataset) = col.names

# clustering = read.csv(clustering.path)

# # get the unique labes
# labels = unique(clustering[2])
# labels = as.vector(unlist(labels))
# labels = sort(labels, decreasing = FALSE)
# feature_count = length(dataset)

# # get centroids 
# centroids = get_centroids(dataset, clustering, labels)

# # for each cluster:
# for (label in labels)
# {
#   # get list of points in that cluster
#   in.points = dataset[clustering[2] == label,]
  
#   # get list of other points
#   out.points = dataset[clustering[2] != label, ]
  
  
#   # for each in point, compute in and out distances
#   for(point in in.points)
#   {
#     inner.distance = 0
#     outer.distance = 0 # average distance to 
    
#     for (in.index in 1:nrow(in.points))
#     {
#       in.vector = in.points[in.index, 2:feature_count]
#       for (out.index in 1:nrow(out.points))
#       {
#         out.vector = out.points[out.index, 2:feature_count]
#         outer.distance = outer.distance + get_euclidean_distance(in.vector, out.vector)
#       }
#       ouder.distance = outer.distance / nrow(out.points)
#     }
#   }
  
# }

# get_euclidean_distance <- function(vector1, vector2)
# {
#   if (length(vector1) != length(vector2))
#     stop("different vector lengths")
  
#   distance = 0
  
#   for (i in 1:length(vector1))
#     distance = distance + (vector2[i]-vector1[i])**2
  
  
#   return (sqrt(distance))
# }

# # returns a matrix of centroids for the clusters
# get_centroids <- function(dataset, clustering, labels)
# {
#   dimensions = ncol(dataset)
#   centroids = matrix(0L, nrow = 0, ncol = dimensions-1)

#   for (label in labels)
#   {
#     # get the sids of points in this cluster
#     sids = unlist(clustering[clustering[2] == 1,][1], use.names = FALSE)
#     #in.points = dataset[match(sids, dataset[1]), ]
#     in.points = subset(dataset, sid %in% sids)
    
#     #compute average in each dimension
#     centroid = sapply(in.points[,2:dimensions], mean)
#     centroids <- rbind(centroids, centroid)
#   }
  
#   return(centroids)
# }
