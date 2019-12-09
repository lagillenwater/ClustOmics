#' center_and_scale
#'
#' This function filters omics data by signiificant non-parametric associations between omic features and  clinical/demographic variables.
#' @param tbl - data table to centered and scaled (by column)
#' @return centered and scaled data frame
#' @export



# Autoscaling to unit variance.
# Standardize based on spread for comparisons based on correlations.
# By column.
center_and_scale <- function(tbl){
   #tm <- as.matrix(tbl);
   for(coli in 1:ncol(tbl)){
      vec <- tbl[,coli];
      tbl[,coli] <- (vec - mean(vec)) / sd(vec);
   }
   return(tbl);
}
