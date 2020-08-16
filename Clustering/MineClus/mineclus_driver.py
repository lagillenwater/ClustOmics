#%%
from mineclus import MineClus
from sklearn.metrics import silhouette_score, silhouette_samples
from sklearn.decomposition import PCA 
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
import copy
from matplotlib.cm import get_cmap
import os

class MineClusDriver:  
    base_path = os.getcwd()
    clustering_folder = f'{base_path}/clustering'
    clustering_plot_folder = f'{base_path}/clustering_scatter'


    '''
    this function loads the datset to a dictionary, sid=>dimension=>values, also returns a list of dimension names
    input: 
    - dataset path
    outputs:
    - output1: dataset as a dictionary, sid => dimension => value
    - output2: list of dimension names
    - output3: list of ids
    '''
    def load_dataset(self, path):
        dataset = {}
        id_values={}
        
        with open(path) as reader:
            file = reader.readlines()
            dimensions = file[0].split(',')[1:] #skip the sid column
            dimensions[-1] = dimensions[-1].rstrip("\n") # remove the linebreak
            
        for line in file[1:]: # skip the header line
            values = [a.strip() for a in line.split(',')]
            id = values[0]
            dataset[id]={}
            id_values[id] = []
            
            for i in range(1, len(values)):
                value = float(values[i])
                dimension = dimensions[i-1]
                dataset[id][dimension] = value
                id_values[id].append(value)

        return dataset, dimensions, id_values


    '''
    This function generates the principal components for the dataset
    inputs: 
    - dataset_path: path to the csv dataset
    - id_column_name: name of the subject id column. it should be the first columns in the dataset
    output:
    - a dictionary, id=>[0]: first PC, [1]: second PC
    '''
    def get_pcs(self, dataset_path, id_column_name):
        df = pd.read_csv(dataset_path) 
        sids = list(df[id_column_name])
        X = np.array(df)
        X = X[:,1:]
        X = np.array(X, dtype = 'float')

        pca = PCA(n_components=2)
        dataset_pcs = pca.fit_transform(X)

        id_pcs_map = {}
        for i in range(len(sids)):
            id_pcs_map[sids[i]]=dataset_pcs[i]

        return id_pcs_map


    #%%
    '''
    '''
    def visualize_clustering(self, id_pcs_map, id_label_map, centroids, title):
        plt.title(title, fontsize=20)

        outlier_sids = set(id_pcs_map.keys()) - set(id_label_map.keys())
        
        X, Y = ([],[])
        for id in outlier_sids:          
            X.append(id_pcs_map[id][0])
            Y.append(id_pcs_map[id][1])
        plt.scatter(x=X, y=Y, c='white', edgecolors='black', alpha=0.6)

        X, Y, C = ([],[],[])
        for id, label in id_label_map.items():
            X.append(id_pcs_map[id][0])
            Y.append(id_pcs_map[id][1])
            C.append(label)
        plt.scatter(x=X, y=Y, c=C, cmap='tab20b', alpha=0.6)

        X, Y, C = ([],[],[])
        for i in range(len(centroids)):
            id = centroids[i]
            X.append(id_pcs_map[id][0])
            Y.append(id_pcs_map[id][1])
            C.append(i)

        plt.scatter(x=X, y=Y, c=C, cmap='tab20b', alpha=0.8, s=150, edgecolors='red', linewidths=2)
        plt.savefig(f'{self.clustering_plot_folder}/{title}.png')

        plt.show()



    def save_clustering(self, ids, labels, path):
        dataframe = pd.DataFrame({'id': ids, 'labels':labels})
        dataframe.to_csv(path, index=False)


    '''
    '''
    def profile(self, W, dataset_path, id_column='sid', alpha=0.1, beta=0.25):
        mineclus = MineClus()
        dataset, dimensions, id_values = self.load_dataset(dataset_path)
        id_pcs_map = self.get_pcs(dataset_path, id_column)

        for w in W:
            dataset_copy = copy.deepcopy(dataset)
            id_label_map, centroids = mineclus.mine(dataset_copy, dimensions, w)
            
            ids = []
            labels = []
            clustering_data=[]
            for id, label in id_label_map.items():
                ids.append(id)
                labels.append(label)
                clustering_data.append(id_values[id])

            silhouette = round(silhouette_score(clustering_data, labels),3)
            print(f'w={w}, alpha={alpha}, beta={beta}')
            print(f'{len(centroids)} clusters found, silhouette={silhouette}')
            print('--------------------------------------')
            
            title = f'w={round(w,6)} - silhouette={silhouette}'
            self.visualize_clustering(id_pcs_map, id_label_map, centroids, title)
            self.save_clustering(ids, labels, f'{self.clustering_folder}/{title}.csv')
            

#%%
if __name__ == "__main__":
    driver = MineClusDriver()
    W = np.arange(4,4.5, step=0.1)
    driver.profile(W=W, dataset_path='')
