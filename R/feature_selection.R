library(reticulate)

source_python("./inst/SVMRFE.py")

SVMRFE <- function(df, labels, save.ranks=T, annotate=F, plot.cv.scores=F){
    return(SVMRFE_PY(df, labels, save_full_ranks=save.ranks, annotate=annotate, plot_cv_score=plot.cv.scores))
}
