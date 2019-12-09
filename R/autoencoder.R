#' Regression adjustment funcion
#'
#' This function adjusts features for effects of variables with linear regression
#' @param df 
#' @return 
#' @export




autoencoder <- function(df, layer_sizes = c(128, 64, 16), 
                        pretrain_epochs = 100, finetune_epochs = 200,
                        pretrain_lr = 0.01, finetune_lr = 0.00001,
                        batch_size = 100, validation_split = 0.2,
                        use_gpu = FALSE ){
	  # Installation parameters
	library(keras)

	# Model hyperparameters

	if (use_gpu){
	  install_keras(tensorflow = "gpu")
	} else {
	   install_keras(method = "virtualenv", tensorflow = "1.5")
	}

	#use_condaenv("r-tensorflow")
  # Define model
  model <- keras_model_sequential()
  lrelu <- layer_activation_leaky_relu()
  
  # Define encoder layers
  model %>% layer_dense(units = layer_sizes[1], 
                        activation = lrelu, input_shape = ncol(df),
                        name = sprintf("Encoder_%d", 1))
  if (length(layer_sizes) > 1){
    encoder_no <- 2
    for (size in layer_sizes[(2:length(layer_sizes))]){
      model %>% layer_dense(units = size, activation = lrelu, 
                            name = sprintf("Encoder_%d", encoder_no))
      encoder_no <- encoder_no + 1
    }
    
    # Define decoder layers
    decoder_no <- length(layer_sizes)
    for (size in rev(layer_sizes[(1:length(layer_sizes) - 1)])){
      model %>% layer_dense(units = size, activation = "linear", 
                            name = sprintf("Decoder_%d", decoder_no))
      decoder_no <- decoder_no - 1
    }
  }
  model %>% layer_dense(units = ncol(df), activation = "linear",
                        name = sprintf("Decoder_%d", 1))
  
  summary(model)
  
  # Compile model with MSE loss
  model %>% compile(
    loss = "mse",
    optimizer = optimizer_adam(lr = pretrain_lr),
    metrics = "mse"
  )
  
  name_temp <- row.names(df)
  X = as.matrix(df)
  
  # Pretrain model layers
  for (i in seq_along(layer_sizes)) {
    print(sprintf("TRAINING LAYER %d", i))
    freeze_weights(model, from = "Encoder_1", to = "Decoder_1")
    
    model %>% compile(
      loss = "mse",
      optimizer = optimizer_adam(lr = pretrain_lr),
      metrics = "mse"
    )
    
    unfreeze_weights(get_layer(model, name = sprintf("Encoder_%d", i)))
    unfreeze_weights(get_layer(model, name = sprintf("Decoder_%d", i)))
    
    model %>% compile(
      loss = "mse",
      optimizer = optimizer_adam(lr = pretrain_lr),
      metrics = "mse"
    )
    
    history <- model %>% fit(
      X, X, 
      epochs = pretrain_epochs, batch_size = batch_size, 
      validation_split = validation_split
    )
    
    plot(history)
  }
  
  # Finetune entire model
  print("TRAINING ALL LAYERS IN COMBINATION ")
  unfreeze_weights(model, from = "Encoder_1", to = "Decoder_1")
  model %>% compile(
    loss = "mse",
    optimizer = optimizer_adam(lr = finetune_lr),
    metrics = "mse"
  )
  
  history <- model %>% fit(
    X, X, 
    epochs = pretrain_epochs, batch_size = batch_size, 
    validation_split = validation_split
  )
  
  plot(history)
  
  embeddings <- model %>% predict(X)
  
  out_df <- as.data.frame(embeddings)
  row.names(out_df) <- name_temp
  return(out_df)
}




