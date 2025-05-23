% Generated by roxygen2: do not edit by hand
% Please edit documentation in R/autoencoder.R
\name{autoencoder}
\alias{autoencoder}
\title{Regression adjustment funcion}
\usage{
autoencoder(
  df,
  layer_sizes = c(128, 64, 16),
  pretrain_epochs = 100,
  finetune_epochs = 200,
  pretrain_lr = 0.01,
  finetune_lr = 1e-05,
  batch_size = 100,
  validation_split = 0.2,
  use_gpu = FALSE
)
}
\arguments{
\item{df}{}
}
\value{

}
\description{
This function adjusts features for effects of variables with linear regression
}
