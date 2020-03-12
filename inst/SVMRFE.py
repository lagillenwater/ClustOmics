#%%
import os
import sys
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from sklearn.metrics import f1_score
from sklearn.svm import LinearSVC
from sklearn.feature_selection import RFECV
from sklearn.model_selection import StratifiedKFold

# annofile = meta/Soma-ProteinNames.xlsx

def label_soma(cols, anno_file="Soma-ProteinNames.xlsx", names_col="TargetFullName", transpose=True):
	if not os.path.exists(anno_file):
		raise Exception("Cannot open annotation file, not found.")

	meta = pd.read_excel(anno_file, header=None)
	if transpose:
		name_cols = meta.iloc[1:, 0]
		meta_ind = meta.iloc[0, 1:]
		meta = meta.iloc[1:, 1:].T
		meta.columns = name_cols
		meta.index = meta_ind

	if names_col not in meta.columns.values:
		raise Exception("No column named '{}' in annotation file".format(names_col))

	names = meta[names_col]

	new_cols = []
	for col in cols:
		if col[0] == 'X' and "_" in col:
			parts = col[1:].split("_")
			first = "-".join((parts[0], parts[1]))
			second = "_".join((first, parts[2]))
			if second in names:
				new_cols.append(names[second])
		else:
			new_cols.append(col)
	return new_cols

#%%

def SVMRFE_PY(df, labels, save_full_ranks=True, annotate=False, plot_cv_score=True):

	feature_rank_file = "Feature_Ranks.csv"
	top_feature_file = "Top_Features_Sorted.csv"

	X, y = df.align(labels, join='inner', axis=0)
	y = np.reshape(y, (-1, ))

	if annotate:
		X.columns = label_soma(X.columns)

	print("Beginning Training:")
	svc = LinearSVC(max_iter=8000)
	rfe = RFECV(estimator=svc, step=1, verbose=0, cv=StratifiedKFold(3), scoring="accuracy", n_jobs=-1)
	rfe.fit(X, y)
	print("Training Complete")

	print("Best Num Features: {}".format(rfe.n_features_))
	print("Selected Features: {}".format(X.columns.values[rfe.get_support()]))

	# transform data and save
	red = pd.DataFrame(rfe.transform(X), columns=X.columns.values[rfe.get_support()])
	red.index = X.index
	red.index.names = X.index.names
	# red.to_csv(sys.argv[3], index=True)

	# sort features by ranking
	if save_full_ranks:
		labs = X.columns.values
		df = pd.DataFrame(labs, columns=["Column"])
		svc.fit(X, y) # fit svc to get coefs of all feats
		df["(Overall)coef^2"] = np.square(svc.coef_.T)
		df["ranks"] = rfe.ranking_
		df["support"] = rfe.get_support()
		ranks = df.sort_values(["ranks"])
		ranks.to_csv(feature_rank_file)
		print("Feature rankings written to: {}".format(feature_rank_file))

	# Plot number of features VS. cross-validation scores
	if plot_cv_score:
		plt.plot(range(len(rfe.grid_scores_)), rfe.grid_scores_)
		plt.xlabel("Number of features remaining")
		plt.ylabel("Cross validation score (f1)")
		plt.xlim(len(rfe.grid_scores_) + 5, -5)
		plt.show()


# %%
if __name__ == "__main__":
	data = pd.read_csv("soma_filtered_test.csv", low_memory=False, delimiter=',', index_col=[0])
	clus = pd.read_csv("soma2_2019-11-23_AE08_k=11.csv", low_memory=False, delimiter=',', index_col=[0])	
	SVMRFE_PY(data, clus)
