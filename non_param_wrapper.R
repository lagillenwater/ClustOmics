# This function filters omics data by signiificant non-parametric associations between omic features and  clinical/demographic # # variables. 
# It requires the following inputs:
# dat - omics data, preprocesssed based on the type of dataset   ### Add info about data organization and example. 
# pheno_tbl - phenotype data including only variables of interest.  ### Add info about data organization and example. 
# fdr = fdr q value threshold for determining significance. Default is 1e-5. #### Define why this value was chosen. 




non_param_wrapper <- function(dat, pheno_tbl, fdr = 1e-5){

   # Natural logarithm for skewed clinical data.
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
          theseClust <- dat[-w_nas];
          theseVals <- pheno_tbl[-w_nas,coli];
          
          
        } else{
          # or use all of them
          theseClust <- dat;
          theseVals <- pheno_tbl[,coli];
          
        }
        

        
        # Categorical or numeric?
        # If it's non-numeric or has 7 or fewer unique values, assume it's categorical.
        if( (!is.numeric(theseVals) && !is.integer(theseVals)) || length(unique(theseVals)) <= 7){
          
          theseVals <- factorToLevels(theseVals);
          
          
          if(length(unique(theseVals)) < 2){  next;     }
          
          if(length(unique(theseVals)) == 2 ){
            if (!(table(theseVals)[1] == 1  | table(theseVals)[2] == 1)) {
                
              results[[n]] <- cbind(colnames(pheno_tbl[coli]), "Wilcoxon",wilcox.test(theseClust~theseVals)$p.value)
               n <- n+1
              
            } else {next; }
            
          }
          
          if(length(unique(theseVals)) >2 & length(unique(theseVals)) != nrow(pheno_tbl)  & length(unique(theseVals)) != nrow(pheno_tbl[pheno_tbl$visitnum ==2, ])+1){
            
           
              results[[n]] <- cbind(colnames(pheno_tbl[coli]), 'Kruskal Wallis',kruskal.test(theseClust ~ theseVals)$p.value)
              
            n <- n+1
          }
            else{  next;      }
          
        } else { 
          # continuous
         
          # Test for skewness. 
          if(shapiro.test(c(theseVals))$p.value < .05 && abs( skewness(c(theseVals)) ) > .55){
            # take log
            theseVals <- log1minusmin(theseVals);
          }
          
          if(length(unique(theseVals)) >2 ){
            
            
              results[[n]] <- cbind(colnames(pheno_tbl[coli]), "Kruskal Wallis",kruskal.test(theseClust ~theseVals)$p.value )
             
           n <- n+1
          } else { next;     }
          
        }
        
      }
      
      results <- data.frame(matrix(unlist(results), nrow=length(results), byrow=T))
      names(results) <- c("clinical_var", "test_used", "pvalue")
      results$pvalue <- as.numeric(levels(results$pvalue)[results$pvalue])
      results$fdr.p <- p.adjust(results$pvalue, method = "fdr", n = length(results$pvalue)*1317 )
      results <- results[order(results$pvalue), ]
      results <- results[results$fdr.p < fdrp, ]
      res <- list()
      clinVar <-paste(results$clinical_var, collapse = " , ")
      res <- cbind(sum(-log10(results$fdr.p)), clinVar)
      return(res)
         
    }
