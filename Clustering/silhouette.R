# this file gets a dataset and a clusterign and returns the overall and individual Silhouette scores
dataset.path = 'iris.csv'
clustering.path = 'clustering.csv'

dataset = read.csv(dataset.path)
clustering = read.csv(clustering.path)

# get the unique labels
labels = unique(clustering[2])
labels = as.vector(unlist(labels))
labels = sort(labels, decreasing = FALSE)
feature_count = length(dataset)

# for each cluster:
for (label in labels)
{
  # get list of points in that cluster
  in.points.ids = dataset[clustering[2] == label,]
  
  # get list of other points (that are not the current cluster)
  out.points.ids = dataset[clustering[2]!= label, ]
  
  # for each in point, compute inner and outer distances
  for(point in in.points.ids)
  {
    inner.distance = 0 # average distance to points in the current cluster
    outer.distance =  rep(0, length(labels)) # min(average distance to other clusters)
    
    for (in.index in 1:nrow(in.points))
    {
      in.vector = in.points[in.index, 2:feature_count]
      for (out.index in 1:nrow(out.points))
      {
        out.vector = out.points[out.index, 2:feature_count]
        outer.distance = outer.distance + get_euclidean_distance(in.vector, out.vector)
      }
      ouder.distance = outer.distance / nrow(out.points)
    }
  }
}

get.euclidean.distance <- function(vector1, vector2)
{
  if (length(vector1)!=length(vector2))
    stop("different vector lengths")
  
  distance = 0.0
  
  for (i in 1:length(vector1))
    distance = distance + (vector2[i]-vector1[i])**2
  
  
  return (sqrt(distance))
}

get.average.distance <- function(point, cluster.points, self.included=FALSE)
{
  distance = 0.0
  for (i in nrow(cluster.points))
    distance = distance + get.euclidean.distance(point, cluster.points[i,])
  point.count = nrow(cluster.points)
  
  if (self.included)
    point.count = point.count - 1
  
  return (distance / point.count)
}

get.outer.distance <- function
