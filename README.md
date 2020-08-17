# ClustOmics V1.0
ClustOmics is ...



# License
To use this package, you need to cite our paper:
- bibtex: TBA
- MLA: TBA


# Requirements



# Folder Structure
* 📁 Pre-processing
* 📁 Clustering
  * 📁 MineClus
* 📁 Clinical Variable Association
* 📝 .gitignore



# Pre-processing



# Clustering
This project contains the implemented algorithms for subspace clustering.

## MineClus
This folder contains the implementation of the <a href="https://ieeexplore.ieee.org/abstract/document/1377170?casa_token=tNg7rA1xFPgAAAAA:_6IPITnpOQm-btWkA8GFLZCvfIioG8SvLA2x4P6YLLM-vPLM6o06QT9S8eP1fQ_bZHMD15J-Eoly">MineClus algorithm</a> (M. L. Yu et al.) in Python and contains the following scripts:

| Filename | Description |
| -- | -- |
| _fp_growth.py_ | the modified implementation of the <a href="https://dl.acm.org/doi/abs/10.1145/335191.335372?casa_token=NR5IA2UrWW8AAAAA:22khDvV9TgrvvV8wApmjZpSzFOjJkGkWH1HjQ-FkypCP2iC9lbf-oy8-xD1CFKISe5aC7S98xLZ3">FpGrowth Algorithm</a> by J. Han et al., used by MineClus to systematically find the best subspace. |
| _mineclus.py<i></i>_ | the implementation of the MineClus algorithm in Python. The link to the C# implementation will be added here. |
| _mineclus_driver.py_ | this file contains the scripts to run _minelcus.py<i></i>_ with different parameters. See the Parameters section for more information. |
| _score.py<i></i>_ | this class is used by MineClus to compare different subspaces against each other. The original implementation provided by the paper causes overflow with wide datasets. This class compares the scores without directly calculating the _mu_ score to avoid the possible overflows. |

## Requirements
Requires Python 3.7.7 or higher, and the following Python packages:
- sklearn
- matplotlib
- numpy
- pandas
- copy
- os
- random
- math

## Configuration and Use
### Input
The provided code, accepts a csv file as the input file. The first row must contain the headers. The first column should contain the subject ids (sid). The following columns must contain only numbers (integer of floating point) and must not contain any missing values.

### MineClus Parameters
MineClus has three tuning parameters (please refer to the original paper for more information):
- **alpha**: affects the followings: 
  1. defines the minimum number of samples that should be included in a cluster to be considered as a valid cluster (alpha x |D|, where |D| is the total number of subjects in the dataset).
  1. determines the number of random centroids at each iteration (2/alpha). 
- **beta**: tunes the trade-off between the number of subjects and dimensions in a subspace by affecting the mu function as follows: mu = |c| &times; (1/beta)<sup>d</sup>, where |c| indicates the number of the subjects in the subspace and d shows the number of the dimensions/attributes/features/variables.  
- **w**: shows the clustering radius, i.e., the maximum allowed distance from the centroid of a cluster to its members (subjects)

The original paper suggested setting alpha=0.1 and beta=0.25. In order to determine the best value for w, MineClus needs to be executed with different values of w. You can set the values in _mineclus_driver.py_: scroll to the bottom of file and edit the followings:
```python
if __name__ == "__main__":
    driver = MineClusDriver()
    # you should pass an array of values for w.
    W = np.arange(4,4.5, step=0.1)
    # set the path to the dataset. 
    # if alpha and beta are not passed, the default values (0.1 and 0.25) will be used
    driver.profile(W=W, dataset_path='') 
```

### Other Utilities
MineClus generates two types of outputs:
1. Clustering files: by default, the are stored in ./clustering. You should create the output directory in the desired path before running the script. You can modify this path from the top of the _mineclus_driver.py_
```python
    # replace with the desired path
    clustering_folder = f'{base_path}/clustering'
```
2. Scatter plots: generates a 2D scatter plot for the generated clusterings for each w. Note that the first to Principal Components are used to generate the plots. The centroids will be shown using red markers. By default, the plots are stored in ./clustering_scatter. You can modify this path from the top of the _mineclus_driver.py_:
```python
    # replace with the desired path
    clustering_plot_folder = f'{base_path}/clustering_scatter'
```



# Clinical Variable Association



__________________________________________________________________
__________________________________________________________________
#### NB - The following sections are required for submission to bioconductor

### Installation

```r
if (!requireNamespace("devtools", quietly = TRUE))
    install.packages("devtools")

# To install from a private repo, use auth_token with a token
# from https://github.com/settings/tokens. You only need the
# repo scope. Best practice is to save your PAT in env var called
# GITHUB_PAT.
# EXAMPLE: Sys.setenv("GITHUB_PAT" = "abc")


devtools::install_github(repo = "lagillenwater/ClustOmics",  force = TRUE )
```



### Documentation
* tutorial
* reference manual
* News

### Details
* biocViews 
	-"biocViews terms are “keywords” used to describe a given package. They are broadly divided into three categories, representing the type of packages present in the Bioconductor Project - Software, Annotation Data, Experiment Data"
* License 
* Dependencies
* Vignette
* Imports
* LinkingTo
* Suggests 
* SystemRequirements
* Enhances
* URL
* BugReports
* Depends On Me
* Imports Me
* Suggests me
* Links to Me

### Package Archives
* Source Package
* Windows Binary
* Mac OS X
* Source Repository
* Source Repository (Developer Access)
* Package Short Url
* Package Downloads Report
* Old Source Packages for BioC 3.10
