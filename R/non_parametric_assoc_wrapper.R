#' Non-Parametric Association Wrapper Function
#'
#' This function filters omics data by signiificant non-parametric associations between omic features and  clinical/demographic variables.
#' @param dat omics data, preprocesssed based on the type of dataset. Rownames are SIDs
#' @param clin clinical data. Rownames are SIDS.
#' @param padjust function for adjusting nominal p values for multiple comparisons using p.adjust() function from R package Stats. 
#' Defaults to FDR.
#' @param sig_val value for significance after correction for multiple comparisons. Defaults to .05.
#' 
#' @return association results for each feature
#' @export





#  # # variables. 
# It requires the following inputs:
# dat - omics data, preprocesssed based on the type of dataset   ### Add info about data organization and example. 
# pheno_tbl - phenotype data including only variables of interest.  ### Add info about data organization and example. 
# fdr = fdr q value threshold for determining significance. Default is 1e-5. #### Define why this value was chosen. 

# Date: 2019-5-30
# Programmer: Lucas Gillenwater
# Supervisors: Russell Bowler and Katerina Kechris
# Purpose: Program to house functions used in the omic filtering pipelines

non_parametric_assoc_wrapper <- function(dat, clin,  padjust = "fdr", sig_val = .05 ){


    #source('./COPDGene/Code/functions/standardize_wMetabolon.R')
    pheno_tbl <- clin
    #p <- p/length(dat)/dim(clin)[2]
    #pheno_tbl <- pheno_tbl[, c("pctEmph_UL_LL_ratio","FEV1pp_utah", "pctEmph_UpperLobes", "pctEmph_LowerLobes", "AWT_seg_Thirona" ) ]

     log1minusmin <- function(vec){
      # Add 1 to prevent -Inf.
      # If any negative values, first subtract the min.
      if(any(vec < 1)){
        vec <- 1 + vec - min(vec);
      }
      return(log(vec));
    }
         # Max missing data allowed. Skip if greater than this.
    MinPresentPercent <- 90;
    MinPresent <- nrow(pheno_tbl) * MinPresentPercent / 100;
    MaxNAsOK <- nrow(pheno_tbl) * (1 - MinPresentPercent / 100);
    results <- list()
    n <- 1
      # For each clinical variable...
        for(coli in 1:ncol(pheno_tbl)){
                             
          w_nas <- which(is.na(pheno_tbl[,coli]) | pheno_tbl[,coli]=="");
          N_nas <- length(w_nas);
          
          if(N_nas > MaxNAsOK){
            next;
          }
          
          if(N_nas > 0){ 
            # drop these few
            feature <- dat[-w_nas];
            phen <- pheno_tbl[-w_nas,coli];
                      
          } else{
            # or use all of them
            feature <- dat;
            phen <- pheno_tbl[,coli];
            
          }    
          # Categorical or numeric?
          # If it's non-numeric or has 7 or fewer unique values, assume it's categorical.
          if( (!is.numeric(phen) && !is.integer(phen)) || length(unique(phen)) <= 7){
              #phen <- factorToLevels(phen);    
            if(length(unique(phen)) < 2){  next;     }
            if(length(unique(phen)) == 2 ){
              if (!(table(phen)[1] == 1  | table(phen)[2] == 1)) {
                  results[[n]] <- cbind(colnames(pheno_tbl[coli]), "Wilcoxon",wilcox.test(feature~phen)$p.value)
                 n <- n+1
                
              } else {next; }
              
            }
            if(length(unique(phen)) >2 & length(unique(phen)) != nrow(pheno_tbl)  & length(unique(phen)) 
              != nrow(pheno_tbl[pheno_tbl$visitnum ==2, ])+1) {     
                results[[n]] <- cbind(colnames(pheno_tbl[coli]), 'Kruskal Wallis',kruskal.test(feature~phen)$p.value)
                
              n <- n+1
            }
              else{  next;      }
            
          } else { 
            # continuous
           
            # Test for skewness. 
            if(shapiro.test(c(phen))$p.value < .05 && abs( skewness(c(phen)) ) > .55){
              # take log
              phen <- log1minusmin(phen);
            }
            if(length(unique(phen)) >2 ){          
                results[[n]] <- cbind(colnames(pheno_tbl[coli]), "Kendall",cor.test(feature,phen, method = "kendall", use = "complete.obs")$p.value )
               
             n <- n+1
            } else {  next;     }
            
          }
          
        }
        
        results <- data.frame(matrix(unlist(results), nrow=length(results), byrow=T))
        names(results) <- c("clinical_var", "test_used", "pvalue")
        results$pvalue <- as.numeric(levels(results$pvalue)[results$pvalue])
        results <- results[order(results$pvalue), ]
#        results$pvalue <- p.adjust(results$pvalue,method = padjust, n= length(results$pvalue)*numfeat)
 #       results <- results[results$pvalue < sig_val, ]
  #      res <- list()
   #     clinVar <-paste(results$clinical_var, collapse = " , ")
   #     res <- cbind(sum(-log10(results$pvalue)), clinVar)
       
        return(results)
  }
