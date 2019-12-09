# ClustOmics
ClustOmics is an R package for implementing the entire pipeline reported in (citation)

#### NB - The following sections are required for submission to bioconductor

### Installation

```r
if (!requireNamespace("devtools", quietly = TRUE))
    install.packages("devtools")

#To install from a private repo, use auth_token with a token
# from https://github.com/settings/tokens. You only need the
# repo scope. Best practice is to save your PAT in env var called
# GITHUB_PAT.
# EXAMPLE: install_github("hadley/private", auth_token = "abc")


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