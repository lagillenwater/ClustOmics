#' filter_data for deploying association_wrapper 
#'
#' This function filters omics data by signiificant non-parametric associations between omic features and  clinical/demographic variabl
#' @param dat omics data, preprocesssed based on the type of dataset. Rownames are SIDs
#' @param clin clinical features to be used in the filtering. Rownames are SIDS.
#' @param padjust function for adjusting nominal p values for multiple comparisons. 
#' @param filter_function function to be used in filtering. Currently only option in non_parametric.
#' @param parallel logical variable to determine whether to implement parallel processing. Implement if possible due to the memory
#' strain of these variables 
#' @param cores define core numbers. Otherwise it defaults to 2 less thans  available cores
#' @return subsetted filtered data frame 
#' @export


filter_data <- function(dat,clinical, padjust = "fdr", sig_val = .05, filter_function = "non-parametric",  parallel = FALSE, cores = NULL){
	library(parallel)
	library(e1071)
	library(data.table)

	data <- dat
	clin <- clinical

	if(parallel){
		if(is.null(cores)){
			nodes <- detectCores()
			print(sprintf("Detected %s cores", nodes))
			print(sprintf("Using %i cores", (nodes-2)))
			cl <- makeCluster(nodes-2)
		} else {
			print(sprintf("Using %i cores", cores))
			cl <- makeCluster(cores)
		}
		clusterExport(cl, varlist = c("data", "clin", "padjust", "sig_val"), envir = environment())
		clusterEvalQ(cl,library("ClustOmics"))
		clusterEvalQ(cl,library("e1071"))
		print("Testing for associations")
		if(filter_function == "non-parametric"){
			final2 <- parApply(cl, data, 2, function(x) non_parametric_assoc_wrapper(x,clin,padjust, sig_val ))
		}
		print("Finished testing")
		stopCluster(cl)
	} else {
		print("Testing for associations")
			if(filter_function == "non-parametric"){
				final2 <- apply(data,2, function(x) non_parametric_assoc_wrapper(x,clin, padjust, sig_val ))
			}
		print("Finished testing")
		}
	print("Filtering")
	final2 <- mapply(function(x,n) {cbind(x,feature = n)}, x = final2, n = names(final2), SIMPLIFY = FALSE)
	final2 <- rbindlist(final2)
	final2 <- as.data.frame(final2)
	final2$p.adjusted <- p.adjust(final2$pvalue, method = "bonf")
	final2 <- final2[final2$p.adjusted < sig_val,]
	
	rank_p <- function(x){
		val <- sum(-log10(x))
		return(val)
	}

	rank <- aggregate(p.adjusted ~ feature, final2, rank_p )
	clinVars <- aggregate(clinical_var ~ feature, final2, paste)
	final2 <- merge(rank, clinVars, by = "feature")
	names(final2)[2] <-  "sum(-log10(q value)"
	final2 <- final2[order(-final2$"sum(-log10(q value)"),]

	data <- data[,names(data) %in% final2$feature]
	print("Finished filtering")
	return(list(filtered = data, associations = final2))	


}

