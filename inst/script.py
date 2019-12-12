import numpy as np
import pandas as pd
import json
from math import ceil, floor
from sklearn.utils import shuffle

class Cluster():
    def __init__(self, anchr_pt, dims, size, std_dev):
        self.anchr_pt = anchr_pt
        self.num_pts = size
        self.dims = dims
        self.std_dev = std_dev
    def __str__(self):
        return("Cluster with:\n\tAnchor Point at: " + str(self.anchr_pt) 
               + "\n\tDimensions: " + str(self.dims) 
               + "\n\tSize: " + str(self.num_pts) 
               + "\n\tStd Dev: " + str(self.std_dev) + "\n")
    def __repr__(self):
        return("Cluster with:\n\tAnchor Point at: " + str(self.anchr_pt) 
               + "\n\tDimensions: " + str(self.dims) 
               + "\n\tSize: " + str(self.num_pts) 
               + "\n\tStd Dev: " + str(self.std_dev) + "\n")

			   
def generate_clusters(k, mu, s = 2, r = 2, d_size = 100000, d_range = (0,100), d_dim = 100, outliers = 0.05, filename = None, shuffle_rows = True):
    """Function to generate data for subspace clustering.
       Implemented as described in 'Fast Algorithms for Projected Clustering'
       
       k - Number of clusters to generate
       mu - Mean of a Poisson random variable used to choose number of dimensions per cluster.
       s - Scaling factor for generating clustered points in a dimension. 
       r - Spread parameter for generating clustered points in a dimension.
       d_size - Size of data to generate (# of Points/Rows). Default is 100,000.
       d_range - Range of data values. Default is [0-100]. 
       d_dim - Number of dimensions for the data. 
       outliers - Percentage of outliers in decimal form. Default is 0.05 (5%)
       filename - Name of the file to write the data to. If no filename is given, data is not written to a file.
       shuffle_rows - Randomly shuffle the rows of the data before returning/writing to file. Default is True. """
    
            
    # Choose K anchor points uniformly in d_dim dimensional space
    anchor_points = np.random.uniform(low=d_range[0], high=d_range[1]+1, size=(k,d_dim))
    
    # Choose the number of dimensions for each cluster (anchor point) via a 
    # Poisson random variable with mean 'mu'
    cluster_dim_nums = np.random.poisson(lam = mu, size = k)
    
    # Redraw the indices where the number of clusters doesn't fall in the correct range.
    bad_indices = np.where(np.logical_or(cluster_dim_nums < 2,cluster_dim_nums >= d_dim))
    
    # Check for bad indices
    while len(bad_indices[0]) > 0: 
        # Redraw for bad indices
        cluster_dim_nums[bad_indices] = np.random.poisson(lam = mu, size = len(bad_indices))
        # Check for bad redraws
        bad_indices = np.where(np.logical_or(cluster_dim_nums < 2,cluster_dim_nums >= d_dim))
    
    # Choose dimensions for each cluster. 
    cluster_dims = [np.random.choice(d_dim,cluster_dim_nums[0],replace=False)]
    # Set of all dimensions in the data
    dim_set = set(range(d_dim))
    for i in range(1, k):
        # Create an array to hold the dimension IDs
        cluster = np.full(cluster_dim_nums[i], -1)
        # Get the number of dimensions that should be chosen from the previous
        # cluster's set of dimensions
        num_prev_clsts = min(cluster_dim_nums[i-1],cluster_dim_nums[i]//2)
        # Get the number of dimensions that should be chosen as new dimensions.
        num_new_clsts = cluster_dim_nums[i] - num_prev_clsts
        # Choose 'num_prev_clsts' from the previous cluster's dimensions
        prev_dims = np.random.choice(cluster_dims[i-1], num_prev_clsts, replace=False)
        cluster[0:num_prev_clsts] = prev_dims
        # Choose 'num_new_clsts' clusters randomly from the rest of the dimensions
        # i.e. D / cluster_dims[i-1] (The set of all dimensions without the dimensions from the previous cluster)
        cluster[num_prev_clsts:] = np.random.choice(list(dim_set - set(prev_dims)), num_new_clsts, replace=False)
        cluster_dims.append(cluster)
    
    # Realize k exponential random variables
    exp_vars = np.random.exponential(size=k)
    
    # Get the size of the data with the outliers removed
    non_otlr_size = d_size * (1 - outliers)
    
    # Get the points per cluster 
    points_per_clust = (non_otlr_size * (exp_vars / np.sum(exp_vars))).astype(int)
    
    # To ensure that we generate exactly d_size data points, we add
    # pad_num = non_otlr_size - np.sum(points_per_clust) 
    # outliers to the data. The round down operation may leave us with fewer than the desired
    # number of data points, so pad the data to length with outliers. 
    pad_num = non_otlr_size - np.sum(points_per_clust)
    
    # Number of outliers to generate
    outlier_num = d_size - non_otlr_size + pad_num
    
    # Create the data array, which is a matrix of 'd_size' rows by 'd_dim' columns
    cluster_data = np.full((d_size,d_dim),-1,dtype="float32")
    
    # Where to start adding the data in the main array
    start_idx = 0
    # List of final clusters
    clusters = []
    # List of labels for each row (data point)
    labels = np.full(d_size,-1)
    
    for i in range (0,k):
        # Get all dimensions that are not part of the cluster dimensions for this cluster
        non_cluster_dims = ~np.in1d(list(range(d_dim)),cluster_dims[i])
        
        # Assign non-cluster data points randomly
        non_cluster_pts = cluster_data[start_idx:start_idx+points_per_clust[i],non_cluster_dims]
        cluster_data[start_idx:start_idx + points_per_clust[i],non_cluster_dims] = np.random.uniform(low = d_range[0], high = d_range[1],size=non_cluster_pts.shape)
        
        # Array of std_devs for each dimension
        std_devs = []
        
        # Assign labels
        labels[start_idx:start_idx + points_per_clust[i]] = i
        
        # Iterate through dimensions for i-th cluster.
        for j in cluster_dims[i]:
            
            # Choose a scale factor uniformly from the interval [1,s)
            scale_factor = np.random.uniform(low=1.0,high=s)
            
            # Calculate the stddev for the points by multiplying the scale_factor with spread factor 'r'
            std_dev = scale_factor * r
            
            # Add this std_dev to the list.
            std_devs.append(std_dev)
            
            # Select all points in the cluster
            cluster_pts = cluster_data[start_idx:start_idx + points_per_clust[i] , j]
            
            # Generate the cluster data
            cluster_data[start_idx:start_idx + points_per_clust[i] , j] = np.random.normal(loc=anchor_points[i,j], scale=std_dev, size=cluster_pts.shape) 
            
        start_idx += points_per_clust[i]
    
        clusters.append(Cluster(anchor_points[i], cluster_dims[i], points_per_clust[i], std_devs))
        
    
    # Fill in outlier data randomly
    outlier_pts = cluster_data[start_idx:,:]
    cluster_data[start_idx:,:] = np.random.uniform(low = d_range[0], high = d_range[1], size = outlier_pts.shape)

    # Create a DataFrame to hold the data
    out_df = pd.DataFrame({"clust_label" : labels})
    out_df = pd.concat([out_df, pd.DataFrame(cluster_data)],axis='columns')

    # Shuffle the rows of the Dataframe
    if shuffle:
        out_df = shuffle(out_df)

    # Create a DataFrame for info about each cluster (anchor point, # of points, dimensions, etc)
    info_df = pd.DataFrame({"anchor_pt" : list(map(list,anchor_points)), "cluster_dims" : list(map(list,cluster_dims)), "size" : points_per_clust})
    # Name the index variable
    info_df.index.name = "cluster"
    
    # If a filename is specified, write the results to files.
    if filename:        
        out_df.to_csv(filename, index = False)        
        info_df.to_json(filename.split(".")[0] + "_clusters.json")
        
    # Return the results
    return (clusters, out_df, info_df)