# -*- coding: utf-8 -*-
"""
Created on Mon Jun  8 18:31:55 2020

@author: helmis
"""

#%%
from fp_growth import  FpGrowth
from score import Score
import random
import copy
import math

class MineClus:
    alpha = 0.1
    beta = 0.25
    w = 1


    '''
    input: remaining sids, count of centroids, random method
    output: sid of centroids picked at random (uniform distribution)
    '''
    # pick centroids : returns sids
    def pick_centroids(self, sids, centroids_count, random):    
        centroid_indices = set([])
            
        if centroids_count <= 0:
            return []
        
        while len(centroid_indices) < centroids_count:
            random_index = random.randint(0, len(sids)-1)
            if random_index not in centroid_indices:
                centroid_indices.add(random_index)
            
        centroids = [sids[i] for i in centroid_indices]
        
        return centroids
    

    ''' 
    build transactions dim-> close points to centroid
    input:
    output:
    ''' 
    def build_transactions(self, centroid_sid, dataset, dimensions):
        transactions = {}
        for  sid in dataset.keys():
            transactions[sid] = set([])
        
        for sid, dimension_value in dataset.items():
            for dimension in dimensions:
                value = dimension_value[dimension]
                if abs(dataset[centroid_sid][dimension] - value) <= self.w:
                    transactions[sid].add(dimension)
            
        return transactions
    

    '''
    '''
    def get_euclidean(self, id1, id2, subspace, dataset):
        distance = 0.0
        for dimension in subspace:
            distance += math.pow(dataset[id1][dimension]-dataset[id2][dimension],2)

        return math.sqrt(distance)


    '''
    '''
    def build_cluster(self, centroid_id, subspace, dataset, sid_label_map, label):
        member_sids = set([])
        for sid in dataset.keys():
            is_close = True
            for dimension in subspace:
                distance = abs(dataset[centroid_id][dimension] - dataset[sid][dimension])
                if distance > self.w:
                    is_close = False
                    break

            if is_close:
                member_sids.add(sid)
                sid_label_map[sid] = label

        return member_sids



    def mine(self, dataset, dimensions, w, alpha=0.1, beta=0.25, seed=1234, seed_increment=11, max_fails=3):
        self.w = w
        self.alpha = alpha
        self.beta = beta

        sid_label_map = {}
        final_centroids = []
        
        #dataset_path = 'soma_plateID_adj_standardized_filtered_2020-03-31_AE08.csv'

        # if len(self.dataset_copy)==0:
        #     dataset, dimensions = mineclus.load_dataset(dataset_path)
        #     mineclus.dataset_copy = copy.deepcopy(dataset)

        # else:
        #     dataset = copy.deepcopy(mineclus.dataset_copy)

        centroid_count = int(2/alpha)
        min_support = int(alpha * len(dataset))
        random.seed(seed)
        failed_iterations = 0

        # print('-------------------------------------------')
        # print(f'alpha={alpha}, beta={beta}, w={w}')
        # print(f'centroids={centroid_count}, min_support={min_support}')
        # print(f'subjects={len(dataset)}, dimensions={len(dimensions)}')

        while (True):
            # check for termintion
            if len(dataset) < max(centroid_count, min_support) or failed_iterations>=max_fails:
                break

            centroid_ids = self.pick_centroids(list(dataset.keys()), centroid_count, random)
            seed += seed_increment

            best_score = Score(0,0) # dimension and support
            best_subspace = []
            best_centroid = ''

            for centroid in centroid_ids:
                transactions = self.build_transactions(centroid, dataset, dimensions)
                fp_growth = FpGrowth()
                centroid_best_score, centroid_best_subspace = fp_growth.mine(transactions, min_support, beta, best_score)
                if centroid_best_score.is_zero() or best_score.is_higher_than(centroid_best_score, beta):
                    pass

                else:
                    best_score = centroid_best_score
                    best_subspace = centroid_best_subspace
                    best_centroid = centroid
                        
            if best_score.is_zero():
                failed_iterations += 1
                
            else:
                failed_iterations = 0
                cluster_sids = self.build_cluster(best_centroid, best_subspace, dataset, sid_label_map, len(final_centroids))
                final_centroids.append(best_centroid)

                # remove the cluster points from dataset
                for sid in cluster_sids:
                    del dataset[sid]
            

        return sid_label_map, final_centroids