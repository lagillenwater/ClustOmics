
   library(e1071) # for skewness()
   library(openxlsx)
   library(ggplot2)
   library(ggbeeswarm)
   library(reshape2)
   library(ggpubr)
   library(plyr)
# Main function for testing clusters for association with clinical variables.
# Creates output files of all association tests, named according to the input cluster file, and clustering method names.
# If any p-values are < printIfPLT, print out clinical variable name and contingency table for categorical variables.
testClusters <- function(cfname=c(),   printIfPLT=0, visitNum = NULL){
# in DATA directory:  
   if(is.null(cfname)){
      #cfname <- "spectral_clusters_soma_v1.csv"; # dim 196 x 1
      #cfname <- "cluster_results_cluster_labeled.csv"; # dim: 178 x 3; this one has more than 2 clusters, good for testing
      #cfname <- "new_metabolite_clusters.csv";
      #cfname <- "all_clusters.csv";
      #cfname <- "v2_rnn_clusters_shifted.csv"
      #cfname = "./cluster_associations/results_4-9/soma2_metab2_concat_no_outliers.csv"
   }
   print(sprintf("Cluster input file: %s", cfname), q=F);
   clust_tbl <- read.table(cfname, sep=",", header=TRUE, as.is=TRUE, row.names = 1, fill=FALSE);
   
   if(!is.null(visitNum)) {
      pheno_tbl <- read.delim("./COPDGene/Data/clinical/COPDGene_P1P2_All_Visit_29sep18.txt")
      imp <- read.xlsx("./COPDGene/Data/clinical/P1P2_Pheno_w_QCT_DataDict_annotated_29sep18_important clinical variables.xlsx")
      if(visitNum == 1) {
         pheno_tbl <- pheno_tbl[pheno_tbl$visitnum == visitNum & pheno_tbl$sid %in% rownames(clust_tbl),]
         rownames(pheno_tbl) <- pheno_tbl$sid
         pheno_tbl <- pheno_tbl[, c(names(pheno_tbl)[names(pheno_tbl) %in% imp$VariableName], "ccenter")]
       }
    else {
         pheno_tbl <- pheno_tbl[pheno_tbl$visitnum == visitNum & pheno_tbl$sid %in% rownames(clust_tbl),]
         rownames(pheno_tbl) <- pheno_tbl$sid
         pheno_tbl <- pheno_tbl[, c(names(pheno_tbl)[names(pheno_tbl) %in% imp$VariableName], "ccenter")]
   }
   } else {

      pheno_file <- "/Bowler/home/gillenwaterl/COPDGene/Data/clinical/P1P2_Pheno_Flat_All_sids_Sep18.txt";
      print(sprintf("Clinical data file: %s", pheno_file), q=F);
      pheno_tbl <- read.table(pheno_file, sep="\t", header=TRUE, as.is=TRUE, row.names = 1, fill=FALSE,  comment.char = "", quote = "\"");
      # watch out for "DON'T KNOW" ' quote character
   }

   # Ensure that we have consistent SIDs.
   clust_tbl <- checkForSIDs(clust_tbl, pheno_tbl, verbose=TRUE);
   #pheno_tbl <- checkForSIDs(pheno_tbl, clust_tbl, verbose=FALSE); # lots, don't print them

   
   pheno_tbl <- pheno_tbl[rownames(pheno_tbl) %in% rownames(clust_tbl),]
   pheno_tbl <- pheno_tbl[match(rownames(clust_tbl),rownames(pheno_tbl)),]
    
   
   if(!all.equal(rownames(clust_tbl), rownames(pheno_tbl))){
      stop("cluster and pheno row names don't match");
   }

   # Max missing data allowed. Skip if greater than this.
   MinPresentPercent <- 90;
   MinPresent <- nrow(pheno_tbl) * MinPresentPercent / 100;
   MaxNAsOK <- nrow(pheno_tbl) * (1 - MinPresentPercent / 100);

   # If zero-based, increment to 1-based.
   if(min(clust_tbl) == 0){
      clust_tbl <- clust_tbl + 1;
   }

   # Each file may contain multiple clustering results (columns).
   for(clustermethodi  in 1:ncol(clust_tbl)){
      N_clusters <- length(unique(clust_tbl[,clustermethodi]));
      if(N_clusters == 1){
         print(sprintf("testClusters: %s, column %i %s contains a single cluster. Skipping.", cfname, clustermethodi, colnames(clust_tbl)[clustermethodi]), q=F);
         next;
      }
      # Check that all cluster indicies are included. (1..N_clusters)
      expected_clusters <- seq(1:N_clusters);
      if(!all(expected_clusters %in% clust_tbl[,clustermethodi])){
         stop(sprintf("testClusters: not all expected clusters represented: %s, column %i", cfname, clustermethodi));
      }
   
      # This will be filled with results.
      test_results <- rep(1, ncol(pheno_tbl));
      names(test_results) <- colnames(pheno_tbl);
      test_used <- rep("not tested", ncol(pheno_tbl));
      per_cluster_mean_or_percent <- rep("not tested", ncol(pheno_tbl));
      #odds_ratio <- rep("not tested", ncol(pheno_tbl));
      

      # For each clinical variable...
      for(coli in 1:ncol(pheno_tbl)){
         # for example: "finalGold_P1"  coli <- 332
         # unique:  3  2  0  4 -1  1 -2 NA
          print(colnames(pheno_tbl[coli]))
         # Don't test visit date.

        
         # if(colnames(pheno_tbl)[coli] == "Visit_Date_P1" || colnames(pheno_tbl)[coli] == "Visit_Date_P2"){
         #    next;
         # }
         w_nas <- which(is.na(pheno_tbl[,coli]) | pheno_tbl[,coli]=="");
         N_nas <- length(w_nas);
         if(N_nas > MaxNAsOK){
            test_used[coli] <- sprintf("Not tested: %i NA > %.2f%%", N_nas, MinPresentPercent);
            next;
         }
         if(N_nas > 0){
            # drop these few
            theseClust <- clust_tbl[-w_nas,clustermethodi];
            theseVals <- pheno_tbl[-w_nas,coli];
            names(theseClust) <- rownames(pheno_tbl)[-w_nas];
            names(theseVals) <- rownames(pheno_tbl)[-w_nas];

         } else{
            # or use all of them
            theseClust <- clust_tbl[,clustermethodi];
            theseVals <- pheno_tbl[,coli];
            names(theseVals) <- rownames(pheno_tbl);
            names(theseClust) <- rownames(pheno_tbl);
         }

         N_patients <- length(theseClust);
         # Categorical or numeric?
         # If it's non-numeric or has 7 or fewer unique values, assume it's categorical.
         if( (!is.numeric(theseVals) && !is.integer(theseVals)) || length(unique(theseVals)) <= 7){
            # categorical
            if(length(unique(theseVals)) < 2){ 
               test_used[coli] <- sprintf("Not tested: %i unique value", length(unique(theseVals)));
               next; # nothing to test
            }
            #print(sprintf("unique: %3i, len:%3i, %s", length(unique(theseVals)), length(theseVals), colnames(pheno_tbl)[coli]), q=F);
            factors <- unique(theseVals);
            contg_table <- matrix(data=0, nrow=length(factors), ncol=N_clusters);
            rownames(contg_table) <- as.character(factors);
          for(subjectname in names(theseClust)){
               f_row <- as.character(theseVals[subjectname]);
           
               f_col <- theseClust[subjectname]
               contg_table[f_row, f_col] <- contg_table[f_row, f_col] + 1;
             
               #contg_table[as.character(theseVals[subjectname]), theseClust[subjectname]] ++ 
            } # allocate each subject to the contingency table
            pvalue <- chiOrFish(contg_table);
            test_results[coli] <- pvalue;
            test_used[coli] <- sprintf("%s %i categories", names(pvalue), length(factors));
            per_cluster_mean_or_percent[coli] <- get_percent_per_clust_string(contg_table);
            #odds_ratio[coli] <- (contg_table[1,1]*contg_table[2,2])/(contg_table[1,2]*contg_table[2,1])
            #per_cluster_mean_or_percent[coli] <- "[contingency table]"

            if(pvalue <= printIfPLT){ 
               print(sprintf("%s p=%f %s", colnames(pheno_tbl)[coli], pvalue, names(pvalue)), q=F);
               #print(factors, q=F);
               print(contg_table, q=F);
            }

         } else{ 
            # continuous
            # Test for skewness.
            if(shapiro.test(c(theseVals))$p.value < .05 && abs( skewness(c(theseVals)) ) > .55){
               # take log
               theseVals <- log1minusmin(theseVals);
            }
            pvalue <- anova_test(theseVals, theseClust);
            test_results[coli] <- pvalue;
            test_used[coli] <- "numeric (ANOVA)";
            per_cluster_mean_or_percent[coli] <- get_avg_per_clust_string(theseVals, theseClust);
#stop("testing");
            #if(pvalue <= printIfPLT){ 
            #   print(sprintf("%s p=%f %s", colnames(pheno_tbl)[coli], pvalue, names(pvalue)), q=F);
            #}

         } # if categorical or continuous

      } # for 
   } #each clinical variable

      #result_mat <- cbind(test_results, clinical_variable=names(test_results), test_used);
      adj_ps <- p.adjust(test_results, method="fdr");
      adj_ps <- as.numeric(adj_ps)
      #result_mat <- cbind(test_results, FDR=adj_ps, clinical_variable=names(test_results), test_used);
      result_mat <- cbind(test_results, FDR=adj_ps, clinical_variable=names(test_results), test_used,  per_cluster_mean_or_percent);
      oi <- order(test_results);
      result_mat <- result_mat[oi,];
      result_mat <- as.data.frame(result_mat)
      result_mat$FDR <- as.numeric(levels(result_mat$FDR))[result_mat$FDR]
      #write.table(x=result_mat, file="test_results.txt", sep="\t", quote=F, row.names=F, col.names=F);
      #outname <- sprintf("%s.assoc.%i", cfname, clustermethodi);
      outname <- sprintf("%s.assoc.%i.xlsx", cfname, clustermethodi);
      #write.table(x=result_mat, file=outname, sep="\t", quote=F, row.names=F, col.names=F);
      require(openxlsx)
      write.xlsx(x=result_mat, file=outname, sep="\t", quote=F, row.names=F, col.names=T);

    

      vars <- c("Age_Enroll", "gender" , "race", "BMI", "smoking_status",  "finalgold_visit")
      dem.vars <- pheno_tbl[, vars]
      dems <- merge(clust_tbl, dem.vars, by = 0)
    

       a <- ggplot(dems, aes(x=as.factor(cluster_label), y=Age_Enroll, group = cluster_label,col=as.factor(cluster_label)))+
            geom_beeswarm()+ 
            geom_boxplot(alpha = .6) + 
            labs(x = "Cluster", y = "") +
            guides(col=guide_legend(title = "Cluster")) +
            ggtitle("Age")

      gen <- as.data.frame(table(dems$cluster_label, dems$gender))
      gen$perc <- NA

      for(i in (1: nrow(gen))) {
         gen[i,"perc"]<- gen[i,"Freq"]/sum(gen[gen$Var1 == gen[i,"Var1" ], "Freq"]) * 100
         }
      

       b <- ggplot(gen, aes(x=as.factor(Var1),y =perc , fill=as.factor(Var2)))+
            geom_bar(stat = "identity")+ 
            labs(x = "Cluster", y = "") +
            guides(fill=guide_legend(title = "Gender"))+
            ggtitle("Gender")



      race <- as.data.frame(table(dems$cluster_label, dems$race))
      race$perc <- NA

      for(i in (1: nrow(race))) {
         race[i,"perc"]<- race[i,"Freq"]/sum(race[race$Var1 == race[i,"Var1" ], "Freq"]) * 100
         }
      

       c <- ggplot(race, aes(x=as.factor(Var1),y =perc , fill=as.factor(Var2)))+
            geom_bar(stat = "identity")+ 
            labs(x = "Cluster", y = "") +
            guides(fill=guide_legend(title = "Race"))+
            ggtitle("Race")


      d <- ggplot(dems, aes(x=as.factor(cluster_label), y=BMI, group = cluster_label,col=as.factor(cluster_label)))+
            geom_beeswarm()+ 
            geom_boxplot(alpha = .6) + 
            labs(x = "Cluster", y ="") +
            guides(col=guide_legend(title = "Cluster"))+
            ggtitle("BMI")


      smok <- as.data.frame(table(dems$cluster_label, dems$smoking_status))
      smok$perc <- NA

      for(i in (1: nrow(smok))) {
         smok[i,"perc"]<- smok[i,"Freq"]/sum(smok[smok$Var1 == smok[i,"Var1" ], "Freq"]) * 100
         }
      

       e <- ggplot(smok, aes(x=as.factor(Var1),y =perc , fill=as.factor(Var2)))+
            geom_bar(stat = "identity")+ 
            labs(x = "Cluster", y = "") +
            guides(fill=guide_legend(title = "Current Smoker"))+
            ggtitle("Smoking")


      gold <- as.data.frame(table(dems$cluster_label, dems$finalgold_visit))
      gold$perc <- NA

      for(i in (1: nrow(gold))) {
         gold[i,"perc"]<- gold[i,"Freq"]/sum(gold[gold$Var1 == gold[i,"Var1" ], "Freq"]) * 100
         }
      

       f <- ggplot(gold, aes(x=as.factor(Var1),y =perc , fill=as.factor(Var2)))+
            geom_bar(stat = "identity")+ 
            labs(x = "Cluster", y = "") +
            guides(fill=guide_legend(title = "GOLD"))+
            ggtitle("GOLD")

      jpeg(file =  sprintf("%s.assoc.%i.jpeg", cfname, clustermethodi),width=18, height=12, units = 'in', res=100 )
       x <- ggarrange(a,b,c,d,e,f, ncol = 3, nrow = 2)
       print(x)
      dev.off()

    # for each cluster result set

} # testClusters

