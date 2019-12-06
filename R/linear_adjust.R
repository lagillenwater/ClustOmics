#' Regression adjustment funcion
#'
#' This function adjusts features for effects of variables with linear regression
#' @param dat omics data, preprocesssed based on the type of dataset. Rownames are SIDs
#' @param clin clinical data. Rownames are SIDS.
#' @param vars variables to regress out
#' @return list of 2 dataframes, the adjusted feature data and the reduced clinical data 
#' @export


linear_adjust <- function(dat,clin, vars) {
	
	
	clin <- clin[rownames(clin) %in% rownames(dat), ]
	clin <- clin[complete.cases(clin[, vars]),]

	tbl <- merge( dat, clin, by = 0)	

	residTbl <- tbl[, 2:(ncol(dat)+1)]
	
  	# Regression for each variable.
   	for(coli in 2:(ncol(dat)+1)){
      model.text <- paste(sprintf("%s ~", names(tbl)[coli]) ,paste(vars,collapse=" + "),collapse=" ")
  	  model.form <- as.formula( model.text )
	  regMod <- lm( model.form, data=tbl )
      residTbl[,coli-1] <- resid(regMod);
   }

   rownames(residTbl) <- tbl$Row.names

   return(list(residuals = residTbl, clinical = clin))

}




