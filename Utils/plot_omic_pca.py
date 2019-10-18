import argparse
from os import path

import numpy as np
import pandas as pd
from sklearn.decomposition import PCA
import matplotlib.pyplot as plt
from matplotlib import rc
import matplotlib as mpl

parser = argparse.ArgumentParser()
parser.add_argument('data', type=str)
parser.add_argument('--sep', type=str, default=None)
parser.add_argument('--row_names', type=int, default=0,
                    help="Index column of both data and cluster (if applicable) files [default: 0]")
parser.add_argument('--clusters', '-c', type=str, default=None, help="Optional path to file containing cluster labels")
parser.add_argument('--dims', choices=['1', '2', '3'], default='2')
parser.add_argument('--plot_title', type=str, default=None, help="Title of generated plot")

args = parser.parse_args()

plt_dims = int(args.dims)

if not path.exists(args.data):
    print("No data file matching: {} found".format(args.data))
    exit(1)

if plt_dims > 3:
    print("Number of dimensions to plot must be in range [1, 3].")
    exit(1)

data = pd.read_csv(args.data, index_col=args.row_names, sep=args.sep)

if args.clusters is not None:
    clusters = pd.read_csv(args.clusters, index_col=args.row_names, sep=args.sep)
    data, clusters = data.align(clusters, join="inner", axis=0)
    clusters = clusters.values[:,0]
else:
    clusters = np.zeros(shape=(data.shape[0]))

pca = PCA(n_components=3)
data_pca = pca.fit_transform(data)

if plt_dims == 1:
    plt.hist(x=data_pca[:, 0])
    if args.plot_title is None:
        plt.title("Histogram of PC 1 for {}".format(args.data))
    else:
        plt.tile(args.plot_title)

elif plt_dims == 2:
    plt.scatter(data_pca[:, 0], data_pca[:, 1], c=clusters)
    plt.xlabel("PC 1")
    plt.ylabel("PC 2")
    if args.plot_title is None:
        plt.title("Scatter Plot of {}".format(args.data))
    else:
        plt.tile(args.plot_title)

if plt_dims == 3:
    from mpl_toolkits.mplot3d import Axes3D
    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')
    ax.scatter(data_pca[:, 0], data_pca[:, 1], data_pca[:, 2], c=clusters)
    ax.set_xlabel("PC 1")
    ax.set_ylabel("PC 2")
    ax.set_zlabel("PC 3")
    if args.plot_title is None:
        plt.title("3D Scatter Plot of {}".format(args.data))
    else:
        plt.tile(args.plot_title)

font = {'family' : 'monospace',
        'weight' : 'bold',
        'size'   : 20}

rc('font', **font)

plt.show()