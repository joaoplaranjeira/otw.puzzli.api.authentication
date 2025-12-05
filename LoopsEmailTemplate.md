# Loops Email Template para OTP

## Nome do Template
**Puzzli - Código de Verificação OTP**

## Configuração do Template

### Assunto do Email
```
O seu código de verificação Puzzli
```

### Conteúdo do Email (HTML)

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Código de Verificação</title>
</head>
<body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5;">
    <table width="100%" cellpadding="0" cellspacing="0" style="background-color: #f5f5f5; padding: 40px 20px;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" style="background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.05);">
                    <!-- Header -->
                    <tr>
                        <td style="padding: 40px 40px 20px 40px; text-align: center;">
                            <h1 style="margin: 0; color: #1a1a1a; font-size: 28px; font-weight: 600;">Puzzli</h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style="padding: 20px 40px;">
                            <p style="margin: 0 0 20px 0; color: #333333; font-size: 16px; line-height: 24px;">
                                O seu código de verificação é:
                            </p>
                            
                            <div style="background-color: #f8f9fa; border: 2px solid #e9ecef; border-radius: 8px; padding: 30px; text-align: center; margin: 30px 0;">
                                <span style="font-size: 36px; font-weight: 700; letter-spacing: 8px; color: #1a1a1a; font-family: 'Courier New', monospace;">
                                    {{otpCode}}
                                </span>
                            </div>
                            
                            <p style="margin: 20px 0 10px 0; color: #666666; font-size: 14px; line-height: 20px;">
                                Este código expira em <strong>10 minutos</strong>.
                            </p>
                            
                            <p style="margin: 10px 0 0 0; color: #666666; font-size: 14px; line-height: 20px;">
                                Se não solicitou este código, por favor ignore este email.
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style="padding: 30px 40px 40px 40px; border-top: 1px solid #e9ecef;">
                            <p style="margin: 0; color: #999999; font-size: 12px; line-height: 18px; text-align: center;">
                                Esta é uma mensagem automática da Puzzli. Por favor não responda a este email.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
```

### Conteúdo do Email (Texto Simples)

```
O seu Código de Verificação Puzzli

O seu código de verificação é: {{otpCode}}

Este código expira em 10 minutos.

Se não solicitou este código, por favor ignore este email.

---
Esta é uma mensagem automática da Puzzli. Por favor não responda a este email.
```

## Variáveis de Dados

No Loops, crie a seguinte variável de dados:
- **Nome da Variável**: `otpCode`
- **Tipo**: String
- **Descrição**: O código OTP de 6 dígitos

## Instruções de Configuração

1. Entre na sua conta Loops em https://app.loops.so
2. Vá para **Transactionals** → **Create Transactional**
3. Nomeie como: "Puzzli - Código de Verificação OTP"
4. Adicione a variável de dados: `otpCode`
5. Copie o template HTML acima para o editor de email
6. Copie o template de texto simples para a versão de texto
7. Guarde e publique o template
8. Copie o **Transactional ID** das configurações do template
9. Atualize o seu `appsettings.json` com o Transactional ID

## Exemplo de Utilização

O serviço de email enviará dados como este:

```json
{
  "transactionalId": "your-transactional-id",
  "email": "utilizador@exemplo.com",
  "dataVariables": {
    "otpCode": "123456"
  }
}
```
